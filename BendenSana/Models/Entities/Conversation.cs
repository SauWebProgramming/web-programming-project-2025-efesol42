using System.ComponentModel.DataAnnotations.Schema;
using BendenSana.Models; // Eğer ApplicationUser Models içindeyse

// 👇 BURASI KRİTİK! Diğer dosyalar buraya bakıyor.

    public class Conversation
    {
        public int Id { get; set; }

        public string BuyerId { get; set; }
        [ForeignKey("BuyerId")]
        public virtual ApplicationUser Buyer { get; set; }

        public string SellerId { get; set; }
        [ForeignKey("SellerId")]
        public virtual ApplicationUser Seller { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastMessageDate { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
    }