using Microsoft.EntityFrameworkCore;
using StajTakipSistemi.Models;

namespace StajTakipSistemi.Data
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore; 
                                                             
    public class ApplicationDbContext : IdentityDbContext 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<StajTakipSistemi.Models.ogrenci> Students { get; set; }
        public DbSet<StajTakipSistemi.Models.InternshipReport> InternshipReports { get; set; }
    }
}