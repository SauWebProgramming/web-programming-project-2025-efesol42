using System;

namespace BendenSana.ViewModels
{
    public class SellerListViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; } // Ad + Soyad
        public string Email { get; set; }
        public string Address { get; set; }  // Adres bilgisi
        public int ProductCount { get; set; } // İstersek kaç ilanı olduğunu da gösterebiliriz
    }
}