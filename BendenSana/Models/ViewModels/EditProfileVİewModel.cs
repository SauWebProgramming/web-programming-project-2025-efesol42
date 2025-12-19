using System.ComponentModel.DataAnnotations;

namespace BendenSana.ViewModels
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; } = default!;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; } = default!;

        [Display(Name = "E-Posta")]
        public string? Email { get; set; } // Readonly olacak

        [Display(Name = "Adres")]
        public string? Address { get; set; } // ApplicationUser'daki Address alanı

        // --- Şifre Değiştirme ---
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        [Compare("NewPassword", ErrorMessage = "Şifreler uyuşmuyor.")]
        public string? ConfirmNewPassword { get; set; }
    }
}