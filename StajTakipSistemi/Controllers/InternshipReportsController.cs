using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StajTakipSistemi.Data;
using StajTakipSistemi.Models;

namespace StajTakipSistemi.Controllers
{
    [Authorize]
    public class InternshipReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public InternshipReportsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        private bool IsAdminOrAcademic()
        {
            return User.IsInRole("Akademisyen") || User.IsInRole("Yönetici");
        }

        private bool IsAuthorized(InternshipReport report)
        {
            if (IsAdminOrAcademic()) return true;
            return report.Student?.Email == User.Identity?.Name;
        }

        private string GetUploadsFolder()
        {
            var webRootPath = _hostEnvironment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            var uploadsFolder = Path.Combine(webRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);
            return uploadsFolder;
        }

        private async Task<string> SaveUploadedFileAsync(InternshipReport report)
        {
            if (report.UploadFile == null || report.UploadFile.Length == 0)
                return null;

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(report.UploadFile.FileName)}";
            var filePath = Path.Combine(GetUploadsFolder(), fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await report.UploadFile.CopyToAsync(stream);

            return fileName;
        }

        private void DeleteUploadedFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return;

            var filePath = Path.Combine(GetUploadsFolder(), fileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }

        private void PrepareStudentSelectList(int? selectedStudentId = null)
        {
            if (IsAdminOrAcademic())
                ViewData["StudentId"] = new SelectList(_context.Students, "Id", "FirstName", selectedStudentId);
        }

        public async Task<IActionResult> Index(string searchString, string statusFilter)
        {
            var reportsQuery = _context.InternshipReports
                .Include(i => i.Student)
                .AsQueryable();

            if (!IsAdminOrAcademic())
            {
                var userName = User.Identity?.Name;
                reportsQuery = reportsQuery.Where(r => r.Student.Email == userName);
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                reportsQuery = reportsQuery.Where(r =>
                    r.Title.Contains(searchString) ||
                    r.Content.Contains(searchString) ||
                    r.Student.FirstName.Contains(searchString) ||
                    r.Student.LastName.Contains(searchString));
            }

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                if (statusFilter == "approved")
                    reportsQuery = reportsQuery.Where(r => r.IsApproved);
                else if (statusFilter == "pending")
                    reportsQuery = reportsQuery.Where(r => !r.IsApproved);
            }

            return View(await reportsQuery.OrderByDescending(r => r.Date).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var report = await _context.InternshipReports
                .Include(i => i.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (report == null) return NotFound();
            if (!IsAuthorized(report)) return Unauthorized();

            return View(report);
        }

        public IActionResult Create()
        {
            PrepareStudentSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Date,Title,Content,StudentId,UploadFile")] InternshipReport report)
        {
            if (!IsAdminOrAcademic())
            {
                var userName = User.Identity?.Name;
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == userName);

                if (student == null)
                    return BadRequest("Profil bulunamadı.");

                report.StudentId = student.Id;
            }

            if (!ModelState.IsValid)
            {
                PrepareStudentSelectList(report.StudentId);
                return View(report);
            }

            var savedFileName = await SaveUploadedFileAsync(report);
            if (!string.IsNullOrWhiteSpace(savedFileName))
                report.FilePath = savedFileName;

            _context.InternshipReports.Add(report);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var report = await _context.InternshipReports
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null) return NotFound();
            if (!IsAuthorized(report)) return Unauthorized();

            PrepareStudentSelectList(report.StudentId);
            return View(report);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Title,Content,StudentId,UploadFile")] InternshipReport report)
        {
            if (id != report.Id) return NotFound();

            var originalReport = await _context.InternshipReports
                .Include(r => r.Student)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (originalReport == null) return NotFound();
            if (!IsAuthorized(originalReport)) return Unauthorized();

            if (!IsAdminOrAcademic() && originalReport.IsApproved)
                return Forbid();

            if (!ModelState.IsValid)
            {
                PrepareStudentSelectList(report.StudentId);
                return View(report);
            }

            originalReport.Date = report.Date;
            originalReport.Title = report.Title;
            originalReport.Content = report.Content;

            if (IsAdminOrAcademic())
                originalReport.StudentId = report.StudentId;

            var savedFileName = await SaveUploadedFileAsync(report);
            if (!string.IsNullOrWhiteSpace(savedFileName))
            {
                DeleteUploadedFile(originalReport.FilePath);
                originalReport.FilePath = savedFileName;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Akademisyen,Yönetici")]
        public async Task<IActionResult> ToggleApprove(int id)
        {
            var report = await _context.InternshipReports.FindAsync(id);

            if (report != null)
            {
                report.IsApproved = !report.IsApproved;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var report = await _context.InternshipReports
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null) return NotFound();
            if (!IsAuthorized(report)) return Unauthorized();

            DeleteUploadedFile(report.FilePath);

            _context.InternshipReports.Remove(report);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DownloadPdf(int? id)
        {
            if (id == null) return NotFound();

            var report = await _context.InternshipReports
                .Include(i => i.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (report == null) return NotFound();
            if (!IsAuthorized(report)) return Unauthorized();

            QuestPDF.Settings.License = LicenseType.Community;

            var pdfDocument = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);

                    page.Header()
                        .Text("STAJ RAPORU")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Blue.Medium);

                    page.Content().Column(x =>
                    {
                        x.Spacing(8);
                        x.Item().Text($"Öğrenci: {report.Student?.FirstName} {report.Student?.LastName}").FontSize(14).Bold();
                        x.Item().Text($"Konu: {report.Title}");
                        x.Item().Text(report.Content ?? string.Empty);
                        x.Item().PaddingTop(20).Text($"Puan: {report.Score ?? 0} / 100");
                    });
                });
            });

            return File(pdfDocument.GeneratePdf(), "application/pdf", $"StajRaporu_{report.Id}.pdf");
        }
    }
}
