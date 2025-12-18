using BendenSana.Models;

namespace BendenSana.ViewModels
{
    public class CheckoutViewModel
    {
        // Sepet Özeti
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }

        // Ödeme Bilgileri (Simülasyon)
        public string CardName { get; set; } = default!;
        public string CardNumber { get; set; } = default!;
        public string ExpirationDate { get; set; } = default!;
        public string Cvv { get; set; } = default!;

        // Adres Seçimi
        // Address.cs dosyasını gördük, direkt kullanabiliriz.
        public List<Address> UserAddresses { get; set; } = new List<Address>();
        public int SelectedAddressId { get; set; }

        public string PhoneNumber { get; set; } = default!;
    }
}