using System.ComponentModel.DataAnnotations;

namespace BendenSana.ViewModels
{
    public class AddressViewModel
    {
        [Display(Name = "Posta Kodu")]
        public string? ZipCode { get; set; }

        [Display(Name = "Ülke")]
        public string? Country { get; set; }

        [Display(Name = "Şehir")]
        public string? City { get; set; }

        [Display(Name = "Mahalle")]
        public string? District { get; set; } // Tasarımda Mahalle var

        [Display(Name = "Detaylı Açık Adres")]
        public string? AddressDetail { get; set; }
    }
}