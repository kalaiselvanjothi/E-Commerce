using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.Product;
using ShopVerse.Application.Interfaces;

namespace ShopVerse.API.Controllers;

[ApiController]
[Route("api")]
[Produces("application/json")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService) => _reviewService = reviewService;

    /// <summary>Get paginated reviews for a product</summary>
    [HttpGet("reviews/{productId:guid}")]
    [HttpGet("products/{productId:guid}/reviews")]
    public async Task<IActionResult> GetProductReviews(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _reviewService.GetProductReviewsAsync(productId, page, pageSize);
        return Ok(ApiResponse<PagedResult<ReviewDto>>.Ok(result));
    }

    /// <summary>Submit a review for a product (one per user per product)</summary>
    [HttpPost("reviews/{productId:guid}")]
    [HttpPost("products/{productId:guid}/reviews")]
    [Authorize]
    public async Task<IActionResult> CreateReview(Guid productId, [FromBody] CreateReviewDto dto)
    {
        var review = await _reviewService.CreateReviewAsync(GetUserId(), productId, dto);
        return CreatedAtAction(nameof(GetProductReviews),
            new { productId },
            ApiResponse<ReviewDto>.Ok(review, "Review submitted"));
    }

    /// <summary>Mark a review as helpful</summary>
    [HttpPost("reviews/{reviewId:guid}/helpful")]
    [Authorize]
    public async Task<IActionResult> MarkHelpful(Guid reviewId)
    {
        await _reviewService.MarkHelpfulAsync(GetUserId(), reviewId);
        return Ok(ApiResponse<object>.Ok(null!, "Marked as helpful"));
    }

    /// <summary>Get all reviews written by the current user</summary>
    [HttpGet("users/me/reviews")]
    [Authorize]
    public async Task<IActionResult> GetMyReviews()
    {
        var result = await _reviewService.GetUserReviewsAsync(GetUserId());
        return Ok(ApiResponse<List<ReviewDto>>.Ok(result));
    }

    // ─── Admin ───────────────────────────────────────────────────────────────

    /// <summary>[Admin] Hide a review</summary>
    [HttpPatch("reviews/{reviewId:guid}/hide")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> HideReview(Guid reviewId)
    {
        await _reviewService.HideReviewAsync(reviewId);
        return Ok(ApiResponse<object>.Ok(null!, "Review hidden"));
    }

    /// <summary>[Admin] Permanently delete a review</summary>
    [HttpDelete("reviews/{reviewId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteReview(Guid reviewId)
    {
        await _reviewService.DeleteReviewAsync(reviewId);
        return Ok(ApiResponse<object>.Ok(null!, "Review deleted"));
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
