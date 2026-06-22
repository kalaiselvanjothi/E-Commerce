using Microsoft.EntityFrameworkCore;
using ShopVerse.Application.DTOs.Cart;
using ShopVerse.Application.DTOs.Product;
using ShopVerse.Application.Interfaces;
using ShopVerse.Domain.Entities;
using ShopVerse.Infrastructure.Data;

namespace ShopVerse.Infrastructure.Services;

public class WishlistService : IWishlistService
{
    private readonly ShopVerseDbContext _context;
    private readonly ICartService _cartService;

    public WishlistService(ShopVerseDbContext context, ICartService cartService)
    {
        _context     = context;
        _cartService = cartService;
    }

    public async Task<WishlistDto> GetWishlistAsync(Guid userId)
    {
        var wishlist = await GetOrCreateWishlistAsync(userId);

        var items = await _context.WishlistItems
            .Include(w => w.Product)
            .Where(w => w.WishlistId == wishlist.Id && w.Product.IsActive && !w.Product.IsDeleted)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WishlistItemDto
            {
                Id             = w.Id,
                ProductId      = w.ProductId,
                ProductName    = w.Product.Name,
                ProductSlug    = w.Product.Slug,
                ProductImage   = w.Product.ThumbnailUrl,
                Brand          = w.Product.Brand,
                Price          = w.Product.Price,
                CompareAtPrice = w.Product.CompareAtPrice,
                DiscountPercent = w.Product.CompareAtPrice.HasValue
                    ? (int?)Math.Round((w.Product.CompareAtPrice.Value - w.Product.Price)
                        / w.Product.CompareAtPrice.Value * 100)
                    : null,
                Stock         = w.Product.StockQuantity,
                AverageRating = w.Product.AverageRating,
                AddedAt       = w.CreatedAt
            })
            .ToListAsync();

        return new WishlistDto
        {
            Id        = wishlist.Id,
            Items     = items,
            ItemCount = items.Count
        };
    }

    public async Task AddToWishlistAsync(Guid userId, Guid productId)
    {
        var wishlist = await GetOrCreateWishlistAsync(userId);

        var exists = await _context.WishlistItems
            .AnyAsync(w => w.WishlistId == wishlist.Id && w.ProductId == productId);

        if (exists) return;

        _context.WishlistItems.Add(new WishlistItem
        {
            WishlistId = wishlist.Id,
            ProductId  = productId
        });
        await _context.SaveChangesAsync();
    }

    public async Task RemoveFromWishlistAsync(Guid userId, Guid productId)
    {
        var wishlist = await GetOrCreateWishlistAsync(userId);
        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.WishlistId == wishlist.Id && w.ProductId == productId);

        if (item == null) return;
        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsInWishlistAsync(Guid userId, Guid productId)
    {
        var wishlist = await _context.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wishlist == null) return false;
        return await _context.WishlistItems
            .AnyAsync(w => w.WishlistId == wishlist.Id && w.ProductId == productId);
    }

    public async Task MoveToCartAsync(Guid userId, Guid productId)
    {
        await _cartService.AddItemAsync(userId, new AddToCartDto { ProductId = productId, Quantity = 1 });
        await RemoveFromWishlistAsync(userId, productId);
    }

    private async Task<Wishlist> GetOrCreateWishlistAsync(Guid userId)
    {
        var wishlist = await _context.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wishlist != null) return wishlist;

        wishlist = new Wishlist { UserId = userId };
        _context.Wishlists.Add(wishlist);
        await _context.SaveChangesAsync();
        return wishlist;
    }
}
