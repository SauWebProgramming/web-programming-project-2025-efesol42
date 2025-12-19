using System.ComponentModel.DataAnnotations;

namespace BendenSana.ViewModels
{
    public class PaymentOptionsViewModel
    {
        [Display(Name = "Ad")]
        public string? FirstName { get; set; }

        [Display(Name = "Soyad")]
        public string? LastName { get; set; }

        [Display(Name = "Kart Numarası")]
        public string? CardNumber { get; set; }

        [Display(Name = "Son Kullanım Tarihi")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Format MM/YY olmalıdır.")]
        public string? ExpiryDate { get; set; }

        [Display(Name = "CVV")]
        [MaxLength(4)]
        public string? Cvv { get; set; }
    }
}