using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Trade_Offers")]
[Index(nameof(TradeCode), IsUnique = true)]
public class TradeOffer
{
    [Key] public int Id { get; set; }

    [Required, MaxLength(50)]
    public string TradeCode { get; set; } = default!;

    [Required] public string OffererId { get; set; } = default!;
    [ForeignKey(nameof(OffererId))] public ApplicationUser Offerer { get; set; } = default!;

    [Required] public string ReceiverId { get; set; } = default!;
    [ForeignKey(nameof(ReceiverId))] public ApplicationUser Receiver { get; set; } = default!;

    public TradeOfferStatus Status { get; set; } = TradeOfferStatus.pending;

    public string? OffererMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<TradeItem> Items { get; set; } = new List<TradeItem>();
}
