using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajTakipSistemi.Data;
using StajTakipSistemi.Models;
using System.Threading.Tasks;
using System.Linq;

namespace StajTakipSistemi.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Akademisyen"))
            {
                ViewBag.TotalStudents = await _context.Students.CountAsync();
                ViewBag.TotalReports = await _context.InternshipReports.CountAsync();
                ViewBag.ApprovedReports = await _context.InternshipReports.CountAsync(r => r.IsApproved);
                ViewBag.PendingReports = await _context.InternshipReports.CountAsync(r => !r.IsApproved);
            }
            else
            {
                var userEmail = User.Identity?.Name;
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == userEmail);

                if (student != null)
                {
                    var studentReports = _context.InternshipReports.Where(r => r.StudentId == student.Id);
                    ViewBag.TotalReports = await studentReports.CountAsync();
                    ViewBag.ApprovedReports = await studentReports.CountAsync(r => r.IsApproved);
                    ViewBag.PendingReports = await studentReports.CountAsync(r => !r.IsApproved);
                }
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}