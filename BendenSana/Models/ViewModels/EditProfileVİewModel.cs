using System.ComponentModel.DataAnnotations;

namespace BendenSana.ViewModels
{
    public class EditProfileViewModel
    {
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Display(Name = "E-Posta")]
        public string Email { get; set; } // Sadece göstermek için, değiştirmeyeceğiz

        // --- Şifre Değiştirme Alanları (Opsiyonel) ---

        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        [Compare("NewPassword", ErrorMessage = "Şifreler birbiriyle uyuşmuyor.")]
        public string? ConfirmNewPassword { get; set; }
    }
}