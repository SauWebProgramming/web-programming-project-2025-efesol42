using BendenSana.Models;

namespace BendenSana.ViewModels
{
    public class SearchViewModel
    {
        // --- SONUÇLAR ---
        public List<Product> Products { get; set; } = new List<Product>();
        public int TotalResults { get; set; }

        // --- FİLTRELER (Seçilenler) ---
        public string? SearchQuery { get; set; } // Layout'taki input buraya yazar

        public List<int> SelectedCategoryIds { get; set; } = new List<int>();

        // Önemli: Renkleri ID olarak tutuyoruz (Hata çıkmaması için)
        public List<int> SelectedColorIds { get; set; } = new List<int>();

        public List<ProductGender> SelectedGenders { get; set; } = new List<ProductGender>();

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SortBy { get; set; } = "newest";

        // --- SAYFA AÇILINCA DOLDURULACAK LİSTELER ---
        // Bunlar veritabanından çekilip buraya koyulacak
        public List<Category> AllCategories { get; set; } = new List<Category>();
        public List<Color> AllColors { get; set; } = new List<Color>();
    }
}