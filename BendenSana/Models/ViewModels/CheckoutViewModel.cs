using BendenSana.Models;
using System.ComponentModel.DataAnnotations;

namespace BendenSana.ViewModels
{
    public class CheckoutViewModel
    {
        // Sepet Verileri
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }

        // Adres Seçimi
        public List<Address> UserAddresses { get; set; } = new List<Address>();
        public int SelectedAddressId { get; set; }

        // Fatura Adresi (Formdan gelen)
        [Required(ErrorMessage = "Ad zorunludur.")]
        public string FirstName { get; set; } = default!;
        [Required(ErrorMessage = "Soyad zorunludur.")]
        public string LastName { get; set; } = default!;
        [Required(ErrorMessage = "Telefon zorunludur.")]
        public string PhoneNumber { get; set; } = default!;
        [Required(ErrorMessage = "Adres zorunludur.")]
        public string AddressLine { get; set; } = default!;
        [Required(ErrorMessage = "Şehir zorunludur.")]
        public string City { get; set; } = default!;
        [Required(ErrorMessage = "Posta kodu zorunludur.")]
        public string ZipCode { get; set; } = default!;

        // --- ÖDEME BİLGİLERİ (Kayıtlı Karttan Otomatik Gelecek) ---

        [Required(ErrorMessage = "Kart üzerindeki isim zorunludur.")]
        [Display(Name = "Kart Sahibi")]
        public string CardHolderName { get; set; } = default!;

        [Required(ErrorMessage = "Kart numarası zorunludur.")]
        [Display(Name = "Kart Numarası")]
        public string CardNumber { get; set; } = default!;

        [Required(ErrorMessage = "Son kullanma tarihi zorunludur.")]
        [Display(Name = "Son Kullanma Tarihi (MM/YY)")]
        public string ExpiryDate { get; set; } = default!;

        [Required(ErrorMessage = "CVV zorunludur.")]
        [Display(Name = "CVV")]
        public string Cvv { get; set; } = default!;
    }
}