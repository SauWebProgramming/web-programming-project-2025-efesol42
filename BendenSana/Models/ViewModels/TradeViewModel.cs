namespace BendenSana.Models.ViewModels
{
        public class TradeViewModel
        {
            public int Id { get; set; }
            public string? TradeCode { get; set; }
            public DateTime CreatedAt { get; set; }
            public string? Status { get; set; }
            public string? PartnerName { get; set; } // Karşı tarafın adı
            public decimal? CashAmount { get; set; } // Ek nakit teklifi
            public int ItemCount { get; set; } // Toplam takas edilen ürün sayısı
        }
}
