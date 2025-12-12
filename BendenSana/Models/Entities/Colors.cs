using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Colors")]
public class Color
{
    [Key] public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = default!;

    [MaxLength(10)]
    public string? HexCode { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
