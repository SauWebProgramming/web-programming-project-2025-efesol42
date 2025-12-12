using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Conversations")]
public class Conversation
{
    [Key] public int Id { get; set; }

    [Required] public string BuyerId { get; set; } = default!;
    [ForeignKey(nameof(BuyerId))] public ApplicationUser Buyer { get; set; } = default!;

    [Required] public string SellerId { get; set; } = default!;
    [ForeignKey(nameof(SellerId))] public ApplicationUser Seller { get; set; } = default!;

    [Required] public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))] public Product Product { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
