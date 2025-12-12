using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Trade_Items")]
public class TradeItem
{
    [Key] public int Id { get; set; }

    [Required] public int TradeId { get; set; }
    [ForeignKey(nameof(TradeId))] public TradeOffer TradeOffer { get; set; } = default!;

    [Required] public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))] public Product Product { get; set; } = default!;

    public TradeItemType? ItemType { get; set; }
}
