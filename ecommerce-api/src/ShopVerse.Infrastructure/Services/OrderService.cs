using Microsoft.EntityFrameworkCore;
using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.Order;
using ShopVerse.Application.Interfaces;
using ShopVerse.Domain.Entities;
using ShopVerse.Domain.Enums;
using ShopVerse.Infrastructure.Data;

namespace ShopVerse.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly ShopVerseDbContext _context;
    private readonly ICartService _cartService;
    private const decimal TaxRate      = 0.18m;
    private const decimal StandardShip = 99m;
    private const decimal ExpressShip  = 199m;
    private const decimal SameDayShip  = 399m;
    private const decimal FreeShipOver = 499m;

    public OrderService(ShopVerseDbContext context, ICartService cartService)
    {
        _context     = context;
        _cartService = cartService;
    }

    public async Task<OrderDetailDto> CreateOrderAsync(Guid userId, CreateOrderDto dto)
    {
        // Validate address belongs to user
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == dto.AddressId && a.UserId == userId && !a.IsDeleted)
            ?? throw new KeyNotFoundException("Shipping address not found.");

        // Load cart
        var cartDto = await _cartService.GetCartAsync(userId);
        if (!cartDto.Items.Any())
            throw new InvalidOperationException("Your cart is empty.");

        // Validate stock and build order items
        var orderItems = new List<OrderItem>();
        foreach (var ci in cartDto.Items)
        {
            var product = await _context.Products.FindAsync(ci.ProductId)
                ?? throw new KeyNotFoundException($"Product '{ci.ProductName}' no longer exists.");

            if (!product.IsActive)
                throw new InvalidOperationException($"'{product.Name}' is no longer available.");

            if (product.StockQuantity < ci.Quantity)
                throw new InvalidOperationException($"Insufficient stock for '{product.Name}'.");

            orderItems.Add(new OrderItem
            {
                ProductId        = ci.ProductId,
                VariantId        = ci.VariantId,
                ProductName      = ci.ProductName,
                ProductThumbnail = ci.ProductImage,
                VariantInfo      = ci.VariantName,
                Quantity         = ci.Quantity,
                UnitPrice        = ci.UnitPrice,
                TotalPrice       = ci.TotalPrice
            });

            // Reserve stock
            product.StockQuantity -= ci.Quantity;
            product.TotalSold     += ci.Quantity;
        }

        // Compute shipping
        var subTotal = cartDto.Subtotal;
        decimal shipping = dto.DeliveryOption switch
        {
            DeliveryOption.Express  => ExpressShip,
            DeliveryOption.SameDay  => SameDayShip,
            _                       => subTotal >= FreeShipOver ? 0 : StandardShip
        };

        // Apply coupon if present
        decimal discount = cartDto.Discount;
        string? couponCode = cartDto.CouponCode ?? dto.CouponCode;

        if (!string.IsNullOrWhiteSpace(dto.CouponCode) && dto.CouponCode != cartDto.CouponCode)
        {
            var validation = await _cartService.ValidateCouponAsync(dto.CouponCode, subTotal);
            if (validation.IsValid)
            {
                discount   = validation.DiscountAmount;
                couponCode = dto.CouponCode.ToUpper();
            }
        }

        // Free shipping coupon
        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == couponCode);
            if (coupon?.Type == CouponType.FreeShipping) shipping = 0;
            if (coupon != null) coupon.UsedCount++;
        }

        var taxable     = Math.Max(0, subTotal - discount);
        var tax         = Math.Round(taxable * TaxRate, 2);
        var total       = taxable + shipping + tax;
        var orderNumber = await GenerateOrderNumberAsync();

        var order = new Order
        {
            OrderNumber       = orderNumber,
            UserId            = userId,
            ShippingAddressId = dto.AddressId,
            Status            = PaymentMethod.COD == dto.PaymentMethod
                                    ? OrderStatus.Confirmed
                                    : OrderStatus.Placed,
            DeliveryOption    = dto.DeliveryOption,
            SubTotal          = subTotal,
            ShippingAmount    = shipping,
            TaxAmount         = tax,
            DiscountAmount    = discount,
            TotalAmount       = total,
            CouponCode        = couponCode,
            Notes             = dto.Notes,
            EstimatedDelivery = EstimateDelivery(dto.DeliveryOption),
            Items             = orderItems,
            StatusHistory     = new List<OrderStatusHistory>
            {
                new() { Status = OrderStatus.Placed, Comment = "Order placed successfully", ChangedAt = DateTime.UtcNow }
            }
        };

        if (dto.PaymentMethod == PaymentMethod.COD)
        {
            order.StatusHistory.Add(new OrderStatusHistory
            {
                Status    = OrderStatus.Confirmed,
                Comment   = "Order confirmed (Cash on Delivery)",
                ChangedAt = DateTime.UtcNow
            });

            _context.Payments.Add(new Payment
            {
                OrderId   = order.Id,
                Amount    = total,
                Method    = PaymentMethod.COD,
                Status    = PaymentStatus.Pending
            });
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Clear cart after order placed
        await _cartService.ClearCartAsync(userId);

        return await GetOrderDetailAsync(userId, order.Id);
    }

    public async Task<PagedResult<OrderListDto>> GetUserOrdersAsync(Guid userId, int page, int pageSize)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt);

        var total = await query.CountAsync();
        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<OrderListDto>
        {
            Items = orders.Select(ToListDto).ToList(), TotalCount = total, PageNumber = page, PageSize = pageSize
        };
    }

    public async Task<OrderDetailDto> GetOrderDetailAsync(Guid userId, Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.ShippingAddress)
            .Include(o => o.StatusHistory.OrderBy(h => h.ChangedAt))
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new KeyNotFoundException("Order not found.");

        return ToDetailDto(order);
    }

    public async Task<OrderDetailDto> CancelOrderAsync(Guid userId, Guid orderId, string reason)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new KeyNotFoundException("Order not found.");

        var cancellable = new[] { OrderStatus.Placed, OrderStatus.Confirmed, OrderStatus.Packed };
        if (!cancellable.Contains(order.Status))
            throw new InvalidOperationException("Order cannot be cancelled at this stage.");

        await using var tx = await _context.Database.BeginTransactionAsync();

        // Restore stock via bulk SQL (avoids EF change-tracking concurrency issues)
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE ""Products"" SET
                ""StockQuantity"" = ""Products"".""StockQuantity"" + oi.""Quantity"",
                ""TotalSold""     = ""Products"".""TotalSold""     - oi.""Quantity""
               FROM ""OrderItems"" oi
               WHERE oi.""OrderId"" = {orderId} AND ""Products"".""Id"" = oi.""ProductId""");

        await _context.Orders
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(o => o.Status,             OrderStatus.Cancelled)
                .SetProperty(o => o.CancellationReason, reason));

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId   = orderId,
            Status    = OrderStatus.Cancelled,
            Comment   = $"Cancelled by customer: {reason}",
            ChangedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        _context.ChangeTracker.Clear();
        return await GetOrderDetailAsync(userId, orderId);
    }

    public async Task<OrderDetailDto> RequestReturnAsync(Guid userId, Guid orderId, string reason)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId)
            ?? throw new KeyNotFoundException("Order not found.");

        if (order.Status != OrderStatus.Delivered)
            throw new InvalidOperationException("Only delivered orders can be returned.");

        await using var tx = await _context.Database.BeginTransactionAsync();

        await _context.Orders
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(o => o.Status,      OrderStatus.ReturnRequested)
                .SetProperty(o => o.ReturnReason, reason));

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId   = orderId,
            Status    = OrderStatus.ReturnRequested,
            Comment   = $"Return requested: {reason}",
            ChangedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        _context.ChangeTracker.Clear();
        return await GetOrderDetailAsync(userId, orderId);
    }

    // ─── Admin ────────────────────────────────────────────────────────────────

    public async Task<PagedResult<OrderListDto>> GetAllOrdersAsync(int page, int pageSize, string? status = null)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Payments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var parsed))
            query = query.Where(o => o.Status == parsed);

        query = query.OrderByDescending(o => o.CreatedAt);

        var total = await query.CountAsync();
        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<OrderListDto>
        {
            Items = orders.Select(ToListDto).ToList(), TotalCount = total, PageNumber = page, PageSize = pageSize
        };
    }

    public async Task<OrderDetailDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto dto)
    {
        var exists = await _context.Orders.AnyAsync(o => o.Id == orderId);
        if (!exists) throw new KeyNotFoundException("Order not found.");

        await using var tx = await _context.Database.BeginTransactionAsync();

        await _context.Orders
            .Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, dto.Status));

        if (dto.Status == OrderStatus.Delivered)
            await _context.Orders
                .Where(o => o.Id == orderId)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.DeliveredAt, DateTime.UtcNow));

        _context.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId   = orderId,
            Status    = dto.Status,
            Comment   = dto.Comment ?? dto.Status.ToString(),
            ChangedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        await tx.CommitAsync();

        _context.ChangeTracker.Clear();
        return await LoadAdminOrderDetailAsync(orderId);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<string> GenerateOrderNumberAsync()
    {
        var today = DateTime.UtcNow;
        var prefix = $"SV{today:yyyyMMdd}";
        var count  = await _context.Orders.CountAsync(o => o.OrderNumber.StartsWith(prefix));
        return $"{prefix}{(count + 1):D4}";
    }

    private static DateTime EstimateDelivery(DeliveryOption option) => option switch
    {
        DeliveryOption.SameDay  => DateTime.UtcNow.AddHours(12),
        DeliveryOption.Express  => DateTime.UtcNow.AddDays(2),
        _                       => DateTime.UtcNow.AddDays(5)
    };

    private async Task<OrderDetailDto> LoadAdminOrderDetailAsync(Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.ShippingAddress)
            .Include(o => o.StatusHistory.OrderBy(h => h.ChangedAt))
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException("Order not found.");
        return ToDetailDto(order);
    }

    private static OrderListDto ToListDto(Order o)
    {
        var payment = o.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
        return new()
        {
            Id            = o.Id,
            OrderNumber   = o.OrderNumber,
            Status        = o.Status.ToString(),
            PaymentStatus = payment?.Status.ToString() ?? "Pending",
            PaymentMethod = payment?.Method.ToString() ?? "COD",
            Total         = o.TotalAmount,
            ItemCount     = o.Items.Count,
            PrimaryImage  = o.Items.FirstOrDefault()?.ProductThumbnail,
            CreatedAt     = o.CreatedAt
        };
    }

    private static OrderDetailDto ToDetailDto(Order o)
    {
        var payment = o.Payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
        return new()
        {
            Id             = o.Id,
            OrderNumber    = o.OrderNumber,
            Status         = o.Status.ToString(),
            PaymentStatus  = payment?.Status.ToString() ?? "Pending",
            PaymentMethod  = payment?.Method.ToString() ?? "COD",
            DeliveryOption = o.DeliveryOption.ToString(),
            Subtotal       = o.SubTotal,
            ShippingCost   = o.ShippingAmount,
            TaxAmount      = o.TaxAmount,
            Discount       = o.DiscountAmount,
            Total          = o.TotalAmount,
            CouponCode     = o.CouponCode,
            EstimatedDelivery = o.EstimatedDelivery?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            RazorpayOrderId   = payment?.RazorpayOrderId,
            CancelReason   = o.CancellationReason,
            ReturnReason   = o.ReturnReason,
            CreatedAt      = o.CreatedAt,
            UpdatedAt      = o.UpdatedAt,
            ShippingAddress = o.ShippingAddress == null ? new() : new ShippingAddressDto
            {
                FullName     = o.ShippingAddress.FullName,
                Phone        = o.ShippingAddress.Phone,
                AddressLine1 = o.ShippingAddress.Line1,
                AddressLine2 = o.ShippingAddress.Line2,
                City         = o.ShippingAddress.City,
                State        = o.ShippingAddress.State,
                Pincode      = o.ShippingAddress.PostalCode,
                Country      = o.ShippingAddress.Country
            },
            Items = o.Items.Select(i => new OrderItemDto
            {
                Id          = i.Id,
                ProductId   = i.ProductId,
                ProductName = i.ProductName,
                ProductSlug = i.Product?.Slug ?? string.Empty,
                ProductImage = i.ProductThumbnail,
                Brand       = i.Product?.Brand ?? string.Empty,
                VariantName = i.VariantInfo,
                Quantity    = i.Quantity,
                UnitPrice   = i.UnitPrice,
                TotalPrice  = i.TotalPrice
            }).ToList(),
            StatusHistory = o.StatusHistory
                .OrderBy(h => h.ChangedAt)
                .Select(h => new StatusHistoryDto
                {
                    Status    = h.Status.ToString(),
                    Note      = h.Comment,
                    CreatedAt = h.ChangedAt
                }).ToList()
        };
    }

}
