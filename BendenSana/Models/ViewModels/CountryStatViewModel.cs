namespace BendenSana.Models.ViewModels
{
    public class CountryStatViewModel
    {
        public string Name { get; set; } = default!;
        public int Value { get; set; }
        public string SEO { get; set; } = default!; // Yüzdelik oran
    }
}
