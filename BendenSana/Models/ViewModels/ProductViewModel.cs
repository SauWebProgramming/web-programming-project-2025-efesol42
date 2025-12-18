namespace BendenSana.Models.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } // Veritabanındaki 'Title' ile eşleşti
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string CategoryName { get; set; }
        public string CoverImageUrl { get; set; } // Listede görünecek ana resim
    }
}