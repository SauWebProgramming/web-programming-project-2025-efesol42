using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Orders")]
[Index(nameof(OrderCode), IsUnique = true)]
public class Order
{
    [Key] public int Id { get; set; }

    [Required, MaxLength(50)]
    public string OrderCode { get; set; } = default!;

    [Required] public string BuyerId { get; set; } = default!;
    [ForeignKey(nameof(BuyerId))] public ApplicationUser Buyer { get; set; } = default!;

    public int? AddressId { get; set; }
    [ForeignKey(nameof(AddressId))] public Address? Address { get; set; }

    public int? CouponId { get; set; }
    [ForeignKey(nameof(CouponId))] public Coupon? Coupon { get; set; }

    public OrderPaymentMethod? PaymentMethod { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ShippingCost { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; } = 0;

    [Required, Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    public OrderStatus? Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
