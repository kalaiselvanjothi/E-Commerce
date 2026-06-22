using Microsoft.EntityFrameworkCore;
using ShopVerse.Application.DTOs.Cart;
using ShopVerse.Application.DTOs.Order;
using ShopVerse.Application.Interfaces;
using ShopVerse.Domain.Entities;
using ShopVerse.Domain.Enums;
using ShopVerse.Infrastructure.Data;

namespace ShopVerse.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly ShopVerseDbContext _context;
    private const decimal TaxRate     = 0.18m;
    private const decimal StandardShip = 99m;
    private const decimal FreeShipOver = 499m;

    public CartService(ShopVerseDbContext context) => _context = context;

    public async Task<CartDto> GetCartAsync(Guid userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return await BuildCartDtoAsync(cart);
    }

    public async Task<CartDto> AddItemAsync(Guid userId, AddToCartDto dto)
    {
        var product = await _context.Products.FindAsync(dto.ProductId)
            ?? throw new KeyNotFoundException("Product not found.");

        if (!product.IsActive || product.IsDeleted)
            throw new InvalidOperationException("Product is not available.");

        if (product.StockQuantity < dto.Quantity)
            throw new InvalidOperationException($"Only {product.StockQuantity} unit(s) in stock.");

        var cart = await GetOrCreateCartAsync(userId);

        // Check if same product+variant already in cart
        var existing = cart.Items.FirstOrDefault(i =>
            i.ProductId == dto.ProductId && i.VariantId == dto.VariantId);

        if (existing != null)
        {
            var newQty = existing.Quantity + dto.Quantity;
            if (newQty > product.StockQuantity)
                throw new InvalidOperationException($"Only {product.StockQuantity} unit(s) in stock.");
            existing.Quantity = newQty;
        }
        else
        {
            _context.CartItems.Add(new CartItem
            {
                CartId    = cart.Id,
                ProductId = dto.ProductId,
                VariantId = dto.VariantId,
                Quantity  = dto.Quantity,
                UnitPrice = product.Price
            });
        }

        await _context.SaveChangesAsync();
        return await BuildCartDtoAsync(cart);
    }

    public async Task<CartDto> UpdateItemAsync(Guid userId, Guid cartItemId, UpdateCartItemDto dto)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId)
            ?? throw new KeyNotFoundException("Cart item not found.");

        if (dto.Quantity == 0)
        {
            _context.CartItems.Remove(item);
        }
        else
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null && dto.Quantity > product.StockQuantity)
                throw new InvalidOperationException($"Only {product.StockQuantity} unit(s) in stock.");

            item.Quantity = dto.Quantity;
        }

        await _context.SaveChangesAsync();
        return await BuildCartDtoAsync(cart);
    }

    public async Task<CartDto> RemoveItemAsync(Guid userId, Guid cartItemId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId)
            ?? throw new KeyNotFoundException("Cart item not found.");

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
        return await BuildCartDtoAsync(cart);
    }

    public async Task<CartDto> ApplyCouponAsync(Guid userId, string code)
    {
        var cart    = await GetOrCreateCartAsync(userId);
        var subTotal = cart.Items.Sum(i => i.Quantity * i.UnitPrice);

        var validation = await ValidateCouponAsync(code, subTotal);
        if (!validation.IsValid)
            throw new InvalidOperationException(validation.Message ?? "Invalid coupon.");

        cart.CouponCode    = code.ToUpper();
        cart.DiscountAmount = validation.DiscountAmount;
        await _context.SaveChangesAsync();
        return await BuildCartDtoAsync(cart);
    }

    public async Task<CartDto> RemoveCouponAsync(Guid userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        cart.CouponCode    = null;
        cart.DiscountAmount = 0;
        await _context.SaveChangesAsync();
        return await BuildCartDtoAsync(cart);
    }

    public async Task ClearCartAsync(Guid userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        _context.CartItems.RemoveRange(cart.Items);
        cart.CouponCode    = null;
        cart.DiscountAmount = 0;
        await _context.SaveChangesAsync();
    }

    public async Task<CouponValidationDto> ValidateCouponAsync(string code, decimal orderTotal)
    {
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == code.ToUpper() && c.IsActive && !c.IsDeleted);

        if (coupon == null)
            return new CouponValidationDto { IsValid = false, Message = "Coupon not found." };

        if (coupon.ExpiresAt.HasValue && coupon.ExpiresAt < DateTime.UtcNow)
            return new CouponValidationDto { IsValid = false, Message = "Coupon has expired." };

        if (coupon.StartsAt.HasValue && coupon.StartsAt > DateTime.UtcNow)
            return new CouponValidationDto { IsValid = false, Message = "Coupon is not yet active." };

        if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit)
            return new CouponValidationDto { IsValid = false, Message = "Coupon usage limit reached." };

        if (coupon.MinOrderAmount.HasValue && orderTotal < coupon.MinOrderAmount)
            return new CouponValidationDto
            {
                IsValid = false,
                Message = $"Minimum order amount of ₹{coupon.MinOrderAmount:N0} required."
            };

        decimal discount = coupon.Type switch
        {
            CouponType.Percentage  => Math.Round(orderTotal * coupon.Value / 100, 2),
            CouponType.FixedAmount => coupon.Value,
            CouponType.FreeShipping => StandardShip,
            _ => 0
        };

        if (coupon.MaxDiscountAmount.HasValue && discount > coupon.MaxDiscountAmount)
            discount = coupon.MaxDiscountAmount.Value;

        discount = Math.Min(discount, orderTotal);

        return new CouponValidationDto
        {
            IsValid        = true,
            Message        = $"Coupon applied: {coupon.Description}",
            DiscountAmount = discount,
            CouponType     = coupon.Type.ToString(),
            CouponValue    = coupon.Value
        };
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Cart> GetOrCreateCartAsync(Guid userId)
    {
        var cart = await _context.Carts
            .Include(c => c.Items).ThenInclude(i => i.Product)
            .Include(c => c.Items).ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart != null) return cart;

        cart = new Cart { UserId = userId };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    private async Task<CartDto> BuildCartDtoAsync(Cart cart)
    {
        // Reload with fresh product data
        await _context.Entry(cart).Collection(c => c.Items).Query()
            .Include(i => i.Product).ThenInclude(p => p.Category)
            .Include(i => i.Variant)
            .LoadAsync();

        var items = cart.Items.Select(i => new CartItemDto
        {
            Id             = i.Id,
            ProductId      = i.ProductId,
            ProductName    = i.Product?.Name ?? string.Empty,
            ProductSlug    = i.Product?.Slug ?? string.Empty,
            ProductImage   = i.Product?.ThumbnailUrl,
            Brand          = i.Product?.Brand ?? string.Empty,
            Quantity       = i.Quantity,
            UnitPrice      = i.Product?.Price ?? i.UnitPrice,
            CompareAtPrice = i.Product?.CompareAtPrice,
            TotalPrice     = (i.Product?.Price ?? i.UnitPrice) * i.Quantity,
            Stock          = i.Product?.StockQuantity ?? 0,
            VariantId      = i.VariantId,
            VariantName    = i.Variant != null ? $"{i.Variant.Name}: {i.Variant.Value}" : null
        }).ToList();

        var subTotal = items.Sum(i => i.TotalPrice);
        var discount = cart.DiscountAmount;

        bool isFreeShipping = false;
        if (cart.CouponCode != null)
        {
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == cart.CouponCode);
            if (coupon?.Type == CouponType.FreeShipping)
            {
                isFreeShipping = true;
                discount = 0;
            }
        }

        var shipping = (isFreeShipping || subTotal >= FreeShipOver || subTotal == 0) ? 0 : StandardShip;
        var taxable  = Math.Max(0, subTotal - discount);
        var tax      = Math.Round(taxable * TaxRate, 2);
        var total    = taxable + shipping + tax;

        return new CartDto
        {
            Id             = cart.Id,
            Items          = items,
            Subtotal       = subTotal,
            ShippingCost   = shipping,
            TaxAmount      = tax,
            Discount       = discount,
            Total          = total,
            CouponCode     = cart.CouponCode,
            CouponDiscount = discount > 0 ? discount : null,
            ItemCount      = items.Sum(i => i.Quantity),
            IsFreeShipping = isFreeShipping
        };
    }
}
