using BendenSana.Models; // Veya Entities, projene göre

namespace BendenSana.ViewModels
{
    public class MakeTradeOfferViewModel
    {
        // Takaslanmak istenen hedef ürün
        public int TargetProductId { get; set; }
        public global::Product TargetProduct { get; set; } // Namespace hatası olmasın diye böyle yaptım

        // Karşılığında teklif edeceğimiz (Kendi) ürünlerimiz
        public List<global::Product> MyProducts { get; set; } = new List<global::Product>();

        // Checkbox ile seçilen ürünlerin ID'leri buraya dolacak
        public List<int> SelectedProductIds { get; set; } = new List<int>();
    }
}