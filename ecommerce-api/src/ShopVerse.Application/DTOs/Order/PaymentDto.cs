using System.ComponentModel.DataAnnotations;

namespace ShopVerse.Application.DTOs.Order;

public class CreateRazorpayOrderDto
{
    [Required] public Guid OrderId { get; set; }
}

public class RazorpayOrderResponseDto
{
    public string RazorpayOrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string KeyId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
}

public class VerifyPaymentDto
{
    [Required] public Guid OrderId { get; set; }
    [Required] public string RazorpayOrderId { get; set; } = string.Empty;
    [Required] public string RazorpayPaymentId { get; set; } = string.Empty;
    [Required] public string RazorpaySignature { get; set; } = string.Empty;
}

public class CouponValidationDto
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? CouponType { get; set; }
    public decimal? CouponValue { get; set; }
}
