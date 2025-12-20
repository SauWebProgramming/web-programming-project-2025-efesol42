

namespace BendenSana.Models.ViewModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string? OrderCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Status { get; set; }
        public decimal SellerTotal { get; set; }
    }
}
