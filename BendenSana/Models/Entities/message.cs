using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Messages")]
public class Message
{
    [Key] public int Id { get; set; }

    [Required] public int ConversationId { get; set; }
    [ForeignKey(nameof(ConversationId))] public Conversation Conversation { get; set; } = default!;

    [Required] public string SenderId { get; set; } = default!;
    [ForeignKey(nameof(SenderId))] public ApplicationUser Sender { get; set; } = default!;

    [Required]
    public string Content { get; set; } = default!;

    public bool IsRead { get; set; } = false;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
