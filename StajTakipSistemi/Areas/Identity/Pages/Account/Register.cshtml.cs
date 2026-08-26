using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using StajTakipSistemi.Data;

namespace StajTakipSistemi.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Lütfen bir rol seçiniz.")]
            [Display(Name = "Kullanıcı Tipi / Rolü")]
            public string Role { get; set; }

            [Required(ErrorMessage = "Ad alanı zorunludur.")]
            [Display(Name = "Ad")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Soyad alanı zorunludur.")]
            [Display(Name = "Soyad")]
            public string LastName { get; set; }

            // Soru işareti (?) ekleyerek bu alanların zorunlu olmadığını sisteme söylüyoruz
            [Display(Name = "Okul Numarası")]
            public string? StudentNumber { get; set; }

            [Display(Name = "Bölüm")]
            public string? Department { get; set; }

            [Display(Name = "Admin Kayıt Kodu")]
            public string? AdminCode { get; set; }

            [Required(ErrorMessage = "E-posta adresi zorunludur.")]
            [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Şifre alanı zorunludur.")]
            [StringLength(100, ErrorMessage = "{0} en az {2} ve en fazla {1} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Şifre")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Şifreyi Doğrula")]
            [Compare("Password", ErrorMessage = "Şifre ve doğrulama şifresi eşleşmiyor.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // 1. GÜVENLİK: Eğer kişi öğrenci değilse, o kırmızı hataları sistemden siliyoruz!
            if (Input.Role != "Öğrenci")
            {
                ModelState.Remove("Input.StudentNumber");
                ModelState.Remove("Input.Department");
            }
            else
            {
                if (string.IsNullOrEmpty(Input.StudentNumber)) ModelState.AddModelError("Input.StudentNumber", "Öğrenciler için okul numarası zorunludur.");
                if (string.IsNullOrEmpty(Input.Department)) ModelState.AddModelError("Input.Department", "Öğrenciler için bölüm bilgisi zorunludur.");
            }

            // 2. GÜVENLİK: Eğer Yönetici seçildiyse Gizli Kod kontrolü yap!
            if (Input.Role == "Yönetici")
            {
                // Gizli kodu buradan değiştirebilirsin
                if (Input.AdminCode != "a39e538d21d7901f")
                {
                    ModelState.AddModelError(string.Empty, "Yetkisiz İşlem: Yönetici kaydı oluşturmak için geçerli bir Admin Kodu girmelisiniz.");
                }
            }

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Kullanıcı şifre ile yeni bir hesap oluşturdu.");

                    if (!string.IsNullOrEmpty(Input.Role))
                    {
                        await _userManager.AddToRoleAsync(user, Input.Role);
                    }

                    if (Input.Role == "Öğrenci")
                    {
                        var newStudent = new StajTakipSistemi.Models.ogrenci
                        {
                            FirstName = Input.FirstName,
                            LastName = Input.LastName,
                            StudentNumber = Input.StudentNumber,
                            Department = Input.Department,
                            Email = Input.Email
                        };
                        _context.Students.Add(newStudent);
                        await _context.SaveChangesAsync();
                    }

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try { return Activator.CreateInstance<IdentityUser>(); }
            catch { throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'."); }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail) throw new NotSupportedException("The default UI requires a user store with email support.");
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}