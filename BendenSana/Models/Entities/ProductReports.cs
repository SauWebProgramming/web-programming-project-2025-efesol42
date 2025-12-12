using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Product_Reports")]
public class ProductReport
{
    [Key] public int Id { get; set; }

    [Required] public string ReporterId { get; set; } = default!;
    [ForeignKey(nameof(ReporterId))] public ApplicationUser Reporter { get; set; } = default!;

    [Required] public int ProductId { get; set; }
    [ForeignKey(nameof(ProductId))] public Product Product { get; set; } = default!;

    [Required, MaxLength(50)]
    public string Reason { get; set; } = default!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
