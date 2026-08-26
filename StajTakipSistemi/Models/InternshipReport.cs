using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace StajTakipSistemi.Models
{
    public class InternshipReport
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tarih alanı zorunludur.")]
        [Display(Name = "Rapor Tarihi")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Konu başlığı zorunludur.")]
        [Display(Name = "Rapor Başlığı")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "İçerik alanı zorunludur.")]
        [Display(Name = "Staj Notları / İçerik")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Onay Durumu")]
        public bool IsApproved { get; set; }

        public string? FilePath { get; set; }

        public string? Feedback { get; set; }

        [Range(0, 100, ErrorMessage = "Puan 0 ile 100 arasında olmalıdır.")]
        [Display(Name = "Değerlendirme Puanı")]
        public int? Score { get; set; }

        [NotMapped]
        [Display(Name = "Dosya Ekle (PDF/Word)")]
        public IFormFile? UploadFile { get; set; }

        
        [Display(Name = "Öğrenci")]
        public int StudentId { get; set; }

        public ogrenci? Student { get; set; }
    }
}