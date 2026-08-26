using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace StajTakipSistemi.Models
{
    public class ogrenci
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad alanı boş bırakılamaz.")]
        [Display(Name = "Öğrenci Adı")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı boş bırakılamaz.")]
        [Display(Name = "Öğrenci Soyadı")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Öğrenci numarası zorunludur.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Öğrenci numarası tam 10 haneli olmalıdır.")]
        [Display(Name = "Okul Numarası")]
        public string StudentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bölüm bilgisi girilmelidir.")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
        public string Email { get; set; } = string.Empty;

        public List<InternshipReport>? Reports { get; set; }
    }
}