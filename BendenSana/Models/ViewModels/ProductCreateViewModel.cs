using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BendenSana.ViewModels
{
    public class ProductCreateViewModel
    {
        // 👇 BU SATIRI MUTLAKA EKLE (Edit sayfası için gerekli)
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

        // 👇 Resimler burada 'Photos' adıyla tutuluyor. View'da da bunu kullanmalısın.
        [Display(Name = "Ürün Fotoğrafları")]
        public List<IFormFile>? Photos { get; set; }
    }
}