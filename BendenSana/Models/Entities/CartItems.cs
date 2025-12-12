using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Cart_Items")]
public class CartItem
{
    [Key] public int Id { get; set; }

    [Required] public int CartId { get; set; }
    [ForeignKey(nameof(CartId))] public Cart Cart { get; set; } = default!;

    [Required] public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))] public Product Product { get; set; } = default!;

    public int Quantity { get; set; } = 1;
}
