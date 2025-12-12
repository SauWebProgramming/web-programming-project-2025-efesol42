using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Order_Items")]
public class OrderItem
{
    [Key] public int Id { get; set; }

    [Required] public int OrderId { get; set; }
    [ForeignKey(nameof(OrderId))] public Order Order { get; set; } = default!;

    [Required] public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))] public Product Product { get; set; } = default!;

    [Required] public string SellerId { get; set; } = default!;
    [ForeignKey(nameof(SellerId))] public ApplicationUser Seller { get; set; } = default!;

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int Quantity { get; set; } = 1;
}
