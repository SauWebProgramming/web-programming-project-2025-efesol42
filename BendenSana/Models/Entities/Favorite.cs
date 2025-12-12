using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Favorites")]
public class Favorite
{
    [Key] public int Id { get; set; }

    [Required] public string UserId { get; set; } = default!;
    [ForeignKey(nameof(UserId))] public ApplicationUser User { get; set; } = default!;

    [Required] public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))] public Product Product { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
