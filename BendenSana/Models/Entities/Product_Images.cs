using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Product_Images")]
public class ProductImage
{
    [Key] public int Id { get; set; }

    [Required] public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))] public Product Product { get; set; } = default!;

    [Required, MaxLength(255)]
    public string ImageUrl { get; set; } = default!;

    public bool IsMain { get; set; } = false;
}
