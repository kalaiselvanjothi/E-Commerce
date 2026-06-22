using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.Order;
using ShopVerse.Application.Interfaces;

namespace ShopVerse.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    public PaymentsController(IPaymentService paymentService) => _paymentService = paymentService;

    /// <summary>
    /// Create a Razorpay order for an existing ShopVerse order.
    /// Returns the keyId, amount (₹), and customerInfo needed by the Razorpay JS SDK.
    /// </summary>
    [HttpPost("razorpay/create")]
    [ProducesResponseType(typeof(ApiResponse<RazorpayOrderResponseDto>), 200)]
    public async Task<IActionResult> CreateRazorpayOrder([FromBody] CreateRazorpayOrderDto dto)
    {
        var result = await _paymentService.CreateRazorpayOrderAsync(GetUserId(), dto.OrderId);
        return Ok(ApiResponse<RazorpayOrderResponseDto>.Ok(result));
    }

    /// <summary>
    /// Verify Razorpay payment signature and capture the payment.
    /// Call this from the frontend after the Razorpay checkout modal succeeds.
    /// </summary>
    [HttpPost("razorpay/verify")]
    [ProducesResponseType(typeof(ApiResponse<OrderDetailDto>), 200)]
    public async Task<IActionResult> VerifyPayment([FromBody] VerifyPaymentDto dto)
    {
        var order = await _paymentService.VerifyAndCapturePaymentAsync(GetUserId(), dto);
        return Ok(ApiResponse<OrderDetailDto>.Ok(order, "Payment verified and order confirmed"));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
