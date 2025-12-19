using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


    [Table("Categories")]
    public class Category
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = default!;

        public int? ParentId { get; set; }
        [ForeignKey(nameof(ParentId))] public Category? Parent { get; set; }

        [MaxLength(255)] public string? ImageUrl { get; set; }

        public ICollection<Category> Children { get; set; } = new List<Category>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

