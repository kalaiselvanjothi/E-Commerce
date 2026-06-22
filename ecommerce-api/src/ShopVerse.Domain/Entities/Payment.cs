using ShopVerse.Domain.Common;
using ShopVerse.Domain.Enums;

namespace ShopVerse.Domain.Entities;

public class Payment : BaseEntity
{
    public string? RazorpayOrderId { get; set; }
    public string? RazorpayPaymentId { get; set; }
    public string? RazorpaySignature { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? FailureReason { get; set; }
    public DateTime? PaidAt { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
}
