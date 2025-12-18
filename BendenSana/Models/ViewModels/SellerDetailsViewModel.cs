using BendenSana.Models; // ApplicationUser ve Product için

namespace BendenSana.ViewModels
{
    public class SellerDetailsViewModel
    {
        public ApplicationUser User { get; set; }
        public List<Product> Products { get; set; }
        public int TotalSalesCount { get; set; } // İleride satış sayısını da gösteririz
    }
}