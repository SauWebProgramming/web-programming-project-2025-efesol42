using BendenSana.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

    public enum TradeOfferStatus
    {
        Pending,
        Accepted,
        Rejected,
        Cancelled
    }

    [Table("Trade_Offers")]
    [Index(nameof(TradeCode), IsUnique = true)]
    public class TradeOffer
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string TradeCode { get; set; } = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

        
        [Required]
        public string OffererId { get; set; } = default!;
        [ForeignKey(nameof(OffererId))]
        public virtual ApplicationUser Offerer { get; set; } = default!;

        [Required]
        public string ReceiverId { get; set; } = default!;
        [ForeignKey(nameof(ReceiverId))]
        public virtual ApplicationUser Receiver { get; set; } = default!;

        public TradeOfferStatus Status { get; set; } = TradeOfferStatus.Pending;

        public string? OffererMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

       
        [Column(TypeName = "decimal(18,2)")]
        public decimal? OfferedCashAmount { get; set; }

  
    public virtual ICollection<TradeItem> Items { get; set; } = new List<TradeItem>();
}
