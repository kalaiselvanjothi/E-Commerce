using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.Cart;
using ShopVerse.Application.Interfaces;

namespace ShopVerse.API.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
[Produces("application/json")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    public CartController(ICartService cartService) => _cartService = cartService;

    /// <summary>Get current user's cart with live price summary</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), 200)]
    public async Task<IActionResult> GetCart()
    {
        var cart = await _cartService.GetCartAsync(GetUserId());
        return Ok(ApiResponse<CartDto>.Ok(cart));
    }

    /// <summary>Add a product (with optional variant) to cart</summary>
    [HttpPost("items")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), 200)]
    public async Task<IActionResult> AddItem([FromBody] AddToCartDto dto)
    {
        var cart = await _cartService.AddItemAsync(GetUserId(), dto);
        return Ok(ApiResponse<CartDto>.Ok(cart, "Item added to cart"));
    }

    /// <summary>Update quantity of a cart item (set 0 to remove)</summary>
    [HttpPut("items/{cartItemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), 200)]
    public async Task<IActionResult> UpdateItem(Guid cartItemId, [FromBody] UpdateCartItemDto dto)
    {
        var cart = await _cartService.UpdateItemAsync(GetUserId(), cartItemId, dto);
        return Ok(ApiResponse<CartDto>.Ok(cart, "Cart updated"));
    }

    /// <summary>Remove a specific item from cart</summary>
    [HttpDelete("items/{cartItemId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), 200)]
    public async Task<IActionResult> RemoveItem(Guid cartItemId)
    {
        var cart = await _cartService.RemoveItemAsync(GetUserId(), cartItemId);
        return Ok(ApiResponse<CartDto>.Ok(cart, "Item removed from cart"));
    }

    /// <summary>Apply a coupon code</summary>
    [HttpPost("coupon")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), 200)]
    public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponDto dto)
    {
        var cart = await _cartService.ApplyCouponAsync(GetUserId(), dto.Code);
        return Ok(ApiResponse<CartDto>.Ok(cart, "Coupon applied"));
    }

    /// <summary>Remove applied coupon</summary>
    [HttpDelete("coupon")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), 200)]
    public async Task<IActionResult> RemoveCoupon()
    {
        var cart = await _cartService.RemoveCouponAsync(GetUserId());
        return Ok(ApiResponse<CartDto>.Ok(cart, "Coupon removed"));
    }

    /// <summary>Validate a coupon code without applying it</summary>
    [HttpGet("coupon/validate")]
    public async Task<IActionResult> ValidateCoupon([FromQuery] string code, [FromQuery] decimal orderTotal = 0)
    {
        var result = await _cartService.ValidateCouponAsync(code, orderTotal);
        return Ok(ApiResponse<object>.Ok(result));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
