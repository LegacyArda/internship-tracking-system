using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajTakipSistemi.Data;
using System.Threading.Tasks;
using System.Linq;

namespace StajTakipSistemi.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity?.Name;

            // Eğer giriş yapan hoca veya admin ise sadece hesap bilgisini gösterir
            if (User.IsInRole("Akademisyen") || User.IsInRole("Yönetici"))
            {
                ViewBag.Role = User.IsInRole("Akademisyen") ? "Akademisyen" : "Yönetici";
                ViewBag.Email = userEmail;
                return View();
            }

            // Eğer giriş yapan öğrenci ise veritabanındaki "ogrenci" tablosundan detayları çeker
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == userEmail);

            if (student == null)
            {
                ViewBag.Role = "Öğrenci";
                ViewBag.Email = userEmail;
                return View();
            }

            ViewBag.Role = "Öğrenci";
            return View(student);
        }
    }
}