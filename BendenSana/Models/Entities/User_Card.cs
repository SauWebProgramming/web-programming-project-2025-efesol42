using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("User_Cards")]
public class UserCard
{
    [Key] public int Id { get; set; }

    [Required] public string UserId { get; set; } = default!;
    [ForeignKey(nameof(UserId))] public ApplicationUser User { get; set; } = default!;

    [Required, MaxLength(100)]
    public string CardHolderName { get; set; } = default!;

    [Required, MaxLength(20)]
    public string CardNumber { get; set; } = default!;

    [MaxLength(5)]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$")]
    public string? ExpiryDate { get; set; }

    [MaxLength(4)]
    [RegularExpression(@"^\d{3,4}$")]
    public string? Cvv { get; set; }

    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
