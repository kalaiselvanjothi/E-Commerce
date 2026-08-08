using Microsoft.EntityFrameworkCore;
using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.Product;
using ShopVerse.Application.Interfaces;
using ShopVerse.Domain.Entities;
using ShopVerse.Infrastructure.Data;

namespace ShopVerse.Infrastructure.Services;

public class ReviewService : IReviewService
{
    private readonly ShopVerseDbContext _context;

    public ReviewService(ShopVerseDbContext context) => _context = context;

    public async Task<PagedResult<ReviewDto>> GetProductReviewsAsync(Guid productId, int page, int pageSize)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && !r.IsHidden)
            .OrderByDescending(r => r.HelpfulCount)
            .ThenByDescending(r => r.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => MapDto(r))
            .ToListAsync();

        return new PagedResult<ReviewDto>
        {
            Items      = items,
            TotalCount = total,
            PageNumber = page,
            PageSize   = pageSize
        };
    }

    public async Task<ReviewDto> CreateReviewAsync(Guid userId, Guid productId, CreateReviewDto dto)
    {
        var product = await _context.Products.FindAsync(productId)
            ?? throw new KeyNotFoundException("Product not found.");

        // One review per product per user
        var exists = await _context.Reviews.AnyAsync(r => r.UserId == userId && r.ProductId == productId);
        if (exists)
            throw new InvalidOperationException("You have already reviewed this product.");

        // Check verified purchase (Must have purchased and received the product)
        var isVerified = await _context.OrderItems
            .Include(oi => oi.Order)
            .AnyAsync(oi => oi.ProductId == productId
                         && oi.Order.UserId == userId
                         && oi.Order.Status == Domain.Enums.OrderStatus.Delivered);

        var review = new Review
        {
            UserId             = userId,
            ProductId          = productId,
            Rating             = dto.Rating,
            Title              = dto.Title,
            Body               = dto.Body,
            IsVerifiedPurchase = isVerified
        };

        _context.Reviews.Add(review);

        // Recalculate product aggregate rating
        await _context.SaveChangesAsync();
        await UpdateProductRatingAsync(productId);

        await _context.Entry(review).Reference(r => r.User).LoadAsync();
        return MapDto(review);
    }

    public async Task MarkHelpfulAsync(Guid userId, Guid reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId)
            ?? throw new KeyNotFoundException("Review not found.");

        review.HelpfulCount++;
        await _context.SaveChangesAsync();
    }

    public async Task<List<ReviewDto>> GetUserReviewsAsync(Guid userId)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => MapDto(r))
            .ToListAsync();
    }

    public async Task HideReviewAsync(Guid reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId)
            ?? throw new KeyNotFoundException("Review not found.");

        review.IsHidden = true;
        await _context.SaveChangesAsync();
        await UpdateProductRatingAsync(review.ProductId);
    }

    public async Task DeleteReviewAsync(Guid reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId)
            ?? throw new KeyNotFoundException("Review not found.");

        var productId = review.ProductId;
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        await UpdateProductRatingAsync(productId);
    }

    private async Task UpdateProductRatingAsync(Guid productId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId && !r.IsHidden)
            .ToListAsync();

        var product = await _context.Products.FindAsync(productId);
        if (product == null) return;

        product.TotalReviews  = reviews.Count;
        product.AverageRating = reviews.Count > 0
            ? Math.Round(reviews.Average(r => r.Rating), 1)
            : 0;

        await _context.SaveChangesAsync();
    }

    private static ReviewDto MapDto(Review r) => new()
    {
        Id                 = r.Id,
        Rating             = r.Rating,
        Title              = r.Title,
        Body               = r.Body,
        ReviewerName       = r.User != null ? $"{r.User.FirstName} {r.User.LastName[0]}." : "Anonymous",
        ReviewerAvatar     = r.User?.AvatarUrl,
        IsVerifiedPurchase = r.IsVerifiedPurchase,
        IsHidden           = r.IsHidden,
        HelpfulCount       = r.HelpfulCount,
        ProductId          = r.ProductId,
        ProductName        = r.Product?.Name ?? string.Empty,
        CreatedAt          = r.CreatedAt
    };
}
