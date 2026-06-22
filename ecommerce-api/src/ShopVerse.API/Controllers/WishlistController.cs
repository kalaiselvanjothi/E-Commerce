using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.Product;
using ShopVerse.Application.Interfaces;

namespace ShopVerse.API.Controllers;

[ApiController]
[Route("api/wishlist")]
[Authorize]
[Produces("application/json")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;
    public WishlistController(IWishlistService wishlistService) => _wishlistService = wishlistService;

    /// <summary>Get current user's wishlist</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<WishlistDto>), 200)]
    public async Task<IActionResult> GetWishlist()
    {
        var wishlist = await _wishlistService.GetWishlistAsync(GetUserId());
        return Ok(ApiResponse<WishlistDto>.Ok(wishlist));
    }

    /// <summary>Add a product to wishlist (idempotent)</summary>
    [HttpPost]
    public async Task<IActionResult> AddToWishlist([FromBody] WishlistAddDto dto)
    {
        await _wishlistService.AddToWishlistAsync(GetUserId(), dto.ProductId);
        return Ok(ApiResponse<object>.Ok(null!, "Added to wishlist"));
    }

    /// <summary>Remove a product from wishlist</summary>
    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> RemoveFromWishlist(Guid productId)
    {
        await _wishlistService.RemoveFromWishlistAsync(GetUserId(), productId);
        return Ok(ApiResponse<object>.Ok(null!, "Removed from wishlist"));
    }

    /// <summary>Check if a specific product is in the wishlist</summary>
    [HttpGet("{productId:guid}/check")]
    public async Task<IActionResult> CheckWishlist(Guid productId)
    {
        var inWishlist = await _wishlistService.IsInWishlistAsync(GetUserId(), productId);
        return Ok(ApiResponse<bool>.Ok(inWishlist));
    }

    /// <summary>Move a wishlist item directly to cart</summary>
    [HttpPost("{productId:guid}/move-to-cart")]
    public async Task<IActionResult> MoveToCart(Guid productId)
    {
        await _wishlistService.MoveToCartAsync(GetUserId(), productId);
        return Ok(ApiResponse<object>.Ok(null!, "Moved to cart"));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
