using ShopVerse.Application.DTOs.Product;

namespace ShopVerse.Application.Interfaces;

public interface IWishlistService
{
    Task<WishlistDto> GetWishlistAsync(Guid userId);
    Task AddToWishlistAsync(Guid userId, Guid productId);
    Task RemoveFromWishlistAsync(Guid userId, Guid productId);
    Task<bool> IsInWishlistAsync(Guid userId, Guid productId);
    Task MoveToCartAsync(Guid userId, Guid productId);
}
