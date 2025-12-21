using System.ComponentModel.DataAnnotations;

namespace BendenSana.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [Display(Name = "Adınız")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email veya Telefon zorunludur.")]
        [Display(Name = "Email veya Telefon Numarası")]
        public string EmailOrPhone { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Password { get; set; }

        public string UserRole { get; set; }
    }
}