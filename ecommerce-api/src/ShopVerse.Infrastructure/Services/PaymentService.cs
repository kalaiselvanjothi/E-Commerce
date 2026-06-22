using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using ShopVerse.Application.DTOs.Order;
using ShopVerse.Application.Interfaces;
using ShopVerse.Domain.Enums;
using ShopVerse.Infrastructure.Data;

namespace ShopVerse.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly ShopVerseDbContext _context;
    private readonly IConfiguration _config;
    private readonly IOrderService _orderService;

    public PaymentService(ShopVerseDbContext context, IConfiguration config, IOrderService orderService)
    {
        _context      = context;
        _config       = config;
        _orderService = orderService;
    }

    public async Task<RazorpayOrderResponseDto> CreateRazorpayOrderAsync(Guid userId, Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new KeyNotFoundException("Order not found.");

        if (order.Status != Domain.Enums.OrderStatus.Placed)
            throw new InvalidOperationException("Payment already processed for this order.");

        var keyId     = _config["Razorpay:KeyId"]     ?? throw new InvalidOperationException("Razorpay KeyId not configured");
        var keySecret = _config["Razorpay:KeySecret"] ?? throw new InvalidOperationException("Razorpay KeySecret not configured");

        var client = new RazorpayClient(keyId, keySecret);

        // Razorpay amount is in paise (₹1 = 100 paise)
        var amountInPaise = (int)(order.TotalAmount * 100);

        var options = new Dictionary<string, object>
        {
            { "amount",   amountInPaise },
            { "currency", "INR" },
            { "receipt",  order.OrderNumber },
            { "notes",    new Dictionary<string, string> { { "orderId", order.Id.ToString() } } }
        };

        var razorpayOrder = client.Order.Create(options);
        var rzpOrderId    = razorpayOrder["id"].ToString();

        // Persist the Razorpay order ID against the pending payment
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Status == PaymentStatus.Pending);

        if (payment == null)
        {
            payment = new Domain.Entities.Payment
            {
                OrderId          = orderId,
                Amount           = order.TotalAmount,
                Method           = PaymentMethod.Razorpay,
                Status           = PaymentStatus.Pending,
                RazorpayOrderId  = rzpOrderId
            };
            _context.Payments.Add(payment);
        }
        else
        {
            payment.RazorpayOrderId = rzpOrderId;
        }

        await _context.SaveChangesAsync();

        return new RazorpayOrderResponseDto
        {
            RazorpayOrderId = rzpOrderId!,
            Amount          = order.TotalAmount,
            Currency        = "INR",
            KeyId           = keyId,
            CustomerName    = $"{order.User.FirstName} {order.User.LastName}",
            CustomerEmail   = order.User.Email!,
            CustomerPhone   = order.User.PhoneNumber,
            OrderNumber     = order.OrderNumber
        };
    }

    public async Task<OrderDetailDto> VerifyAndCapturePaymentAsync(Guid userId, VerifyPaymentDto dto)
    {
        var order = await _context.Orders
            .Include(o => o.Payments)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == userId)
            ?? throw new KeyNotFoundException("Order not found.");

        var keySecret = _config["Razorpay:KeySecret"]
            ?? throw new InvalidOperationException("Razorpay KeySecret not configured");

        // Verify HMAC signature
        var payload   = $"{dto.RazorpayOrderId}|{dto.RazorpayPaymentId}";
        var expected  = ComputeHmacSha256(payload, keySecret);
        var isValid   = expected.Equals(dto.RazorpaySignature, StringComparison.OrdinalIgnoreCase);

        var payment = order.Payments.FirstOrDefault(p => p.RazorpayOrderId == dto.RazorpayOrderId)
            ?? order.Payments.FirstOrDefault(p => p.Status == PaymentStatus.Pending);

        if (payment == null)
            throw new InvalidOperationException("No pending payment found for this order.");

        if (isValid)
        {
            payment.RazorpayPaymentId = dto.RazorpayPaymentId;
            payment.RazorpaySignature = dto.RazorpaySignature;
            payment.Status            = PaymentStatus.Completed;
            payment.PaidAt            = DateTime.UtcNow;

            order.Status = Domain.Enums.OrderStatus.Confirmed;
            order.StatusHistory.Add(new Domain.Entities.OrderStatusHistory
            {
                OrderId   = order.Id,
                Status    = Domain.Enums.OrderStatus.Confirmed,
                Comment   = $"Payment successful (ID: {dto.RazorpayPaymentId})",
                ChangedAt = DateTime.UtcNow
            });
        }
        else
        {
            payment.Status        = PaymentStatus.Failed;
            payment.FailureReason = "Signature verification failed";
        }

        await _context.SaveChangesAsync();

        if (!isValid)
            throw new InvalidOperationException("Payment signature verification failed.");

        return await _orderService.GetOrderDetailAsync(userId, dto.OrderId);
    }

    private static string ComputeHmacSha256(string payload, string secret)
    {
        var key   = Encoding.UTF8.GetBytes(secret);
        var data  = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(key);
        return BitConverter.ToString(hmac.ComputeHash(data))
            .Replace("-", "").ToLower();
    }
}
