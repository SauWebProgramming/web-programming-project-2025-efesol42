using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BendenSana.ViewModels
{
    public class ProductCreateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün başlığı zorunludur.")]
        [Display(Name = "Ürün Başlığı")]
        public string Title { get; set; } = default!;

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = default!;

        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Display(Name = "Fiyat (TL)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Kategori seçmelisiniz.")]
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        [Display(Name = "Ürün Fotoğrafları")]
        public List<IFormFile>? Photos { get; set; }

        // 👇 BU KISIM EKSİKTİ, EKLENDİ
        public List<string> ExistingImageUrls { get; set; } = new List<string>();
    }
}