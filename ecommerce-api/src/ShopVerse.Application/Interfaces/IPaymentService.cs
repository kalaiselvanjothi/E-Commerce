using ShopVerse.Application.DTOs.Order;

namespace ShopVerse.Application.Interfaces;

public interface IPaymentService
{
    Task<RazorpayOrderResponseDto> CreateRazorpayOrderAsync(Guid userId, Guid orderId);
    Task<OrderDetailDto> VerifyAndCapturePaymentAsync(Guid userId, VerifyPaymentDto dto);
    Task ProcessRazorpayWebhookAsync(string rawBody, string signature);
}
