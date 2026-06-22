using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.Product;

namespace ShopVerse.Application.Interfaces;

public interface IReviewService
{
    Task<PagedResult<ReviewDto>> GetProductReviewsAsync(Guid productId, int page, int pageSize);
    Task<ReviewDto> CreateReviewAsync(Guid userId, Guid productId, CreateReviewDto dto);
    Task MarkHelpfulAsync(Guid userId, Guid reviewId);
    Task<List<ReviewDto>> GetUserReviewsAsync(Guid userId);
    // Admin
    Task HideReviewAsync(Guid reviewId);
    Task DeleteReviewAsync(Guid reviewId);
}
