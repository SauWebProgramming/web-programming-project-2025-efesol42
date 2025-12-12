using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Products")]
public class Product
{
    [Key] public int Id { get; set; }

    [Required] public string SellerId { get; set; } = default!;
    [ForeignKey(nameof(SellerId))] public ApplicationUser Seller { get; set; } = default!;

    [Required] public int CategoryId { get; set; }
    [ForeignKey(nameof(CategoryId))] public Category Category { get; set; } = default!;

    public int? ColorId { get; set; }
    [ForeignKey(nameof(ColorId))] public Color? Color { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = default!;

    public string? Description { get; set; }

    [Required, Column(TypeName = "decimal(18,2)"), Range(0.01, 999999999)]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(18,2)"), Range(0.00, 999999999)]
    public decimal? OriginalPrice { get; set; }

    public int StockQty { get; set; } = 1;

    public ProductGender? Gender { get; set; }
    public ProductStatus? Status { get; set; }

    public bool IsFreeShipping { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}
