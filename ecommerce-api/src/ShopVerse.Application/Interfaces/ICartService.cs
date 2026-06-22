using ShopVerse.Application.DTOs.Cart;
using ShopVerse.Application.DTOs.Order;

namespace ShopVerse.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(Guid userId);
    Task<CartDto> AddItemAsync(Guid userId, AddToCartDto dto);
    Task<CartDto> UpdateItemAsync(Guid userId, Guid cartItemId, UpdateCartItemDto dto);
    Task<CartDto> RemoveItemAsync(Guid userId, Guid cartItemId);
    Task<CartDto> ApplyCouponAsync(Guid userId, string code);
    Task<CartDto> RemoveCouponAsync(Guid userId);
    Task ClearCartAsync(Guid userId);
    Task<CouponValidationDto> ValidateCouponAsync(string code, decimal orderTotal);
}
