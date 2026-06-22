using System.ComponentModel.DataAnnotations;
using ShopVerse.Domain.Enums;

namespace ShopVerse.Application.DTOs.Order;

public class CreateOrderDto
{
    [Required] public Guid AddressId { get; set; }
    public DeliveryOption DeliveryOption { get; set; } = DeliveryOption.Standard;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Razorpay;
    public string? CouponCode { get; set; }
    public string? Notes { get; set; }
}

public class CancelOrderDto
{
    [Required][MaxLength(500)] public string Reason { get; set; } = string.Empty;
}

public class ReturnOrderDto
{
    [Required][MaxLength(500)] public string Reason { get; set; } = string.Empty;
}

public class UpdateOrderStatusDto
{
    [Required] public Domain.Enums.OrderStatus Status { get; set; }
    public string? Comment { get; set; }
}
