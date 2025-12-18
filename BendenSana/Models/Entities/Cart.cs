using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



[Table("Carts")]

public class Cart

{
    [Key] public int Id { get; set; }

    [Required] public string UserId { get; set; } = default!;

    [ForeignKey(nameof(UserId))] public ApplicationUser User { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

}