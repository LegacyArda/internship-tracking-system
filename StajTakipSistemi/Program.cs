using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using QuestPDF;
using QuestPDF.Infrastructure;
using StajTakipSistemi.Data;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);


global::QuestPDF.Settings.License = global::QuestPDF.Infrastructure.LicenseType.Community;

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Bağlantı dizesi 'DefaultConnection' bulunamadı.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminYetkisi", policy => policy.RequireRole("Yönetici"));
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    
    string[] roles = { "Öğrenci", "Akademisyen", "Yönetici" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    
    string myEmail = builder.Configuration.GetSection("AdminSettings")["AdminEmail"] ?? "legacy41@gmail.com";
    var adminUser = await userManager.FindByEmailAsync(myEmail);

    if (adminUser == null)
    {
        
        adminUser = new IdentityUser
        {
            UserName = myEmail,
            Email = myEmail,
            EmailConfirmed = true 
        };
        await userManager.CreateAsync(adminUser, "legacy41");
    }
    else
    {
        
        var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
        await userManager.ResetPasswordAsync(adminUser, token, "legacy41");
    }

    
    if (!await userManager.IsInRoleAsync(adminUser, "Yönetici"))
    {
        await userManager.AddToRoleAsync(adminUser, "Yönetici");
    }
}

app.Run();