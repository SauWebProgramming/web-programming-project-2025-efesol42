using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Addresses")]
public class Address
{
    [Key] public int Id { get; set; }

    [Required] public string UserId { get; set; } = default!;
    [ForeignKey(nameof(UserId))] public ApplicationUser User { get; set; } = default!;

    [MaxLength(50)] public string? Title { get; set; }
    [MaxLength(100)] public string? CompanyName { get; set; }
    [MaxLength(50)] public string? Country { get; set; }
    [MaxLength(50)] public string? City { get; set; }
    [MaxLength(255)] public string? AddressLine { get; set; }
    [MaxLength(255)] public string? AddressLine2 { get; set; }
    [MaxLength(20)] public string? ZipCode { get; set; }
}
