using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Coupons")]
[Index(nameof(Code), IsUnique = true)]
public class Coupon
{
    [Key] public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = default!;

    public CouponDiscountType? DiscountType { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? DiscountValue { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public int? UsageLimit { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "active";
}
