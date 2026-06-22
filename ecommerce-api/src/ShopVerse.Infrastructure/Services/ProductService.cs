using Microsoft.EntityFrameworkCore;
using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.Product;
using ShopVerse.Application.Interfaces;
using ShopVerse.Domain.Entities;
using ShopVerse.Infrastructure.Data;

namespace ShopVerse.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly ShopVerseDbContext _context;

    public ProductService(ShopVerseDbContext context) => _context = context;

    public async Task<PagedResult<ProductListDto>> GetProductsAsync(ProductFilterDto filter)
    {
        var query = _context.Products
            .Include(p => p.Category).ThenInclude(c => c.Parent)
            .Where(p => p.IsActive && !p.IsDeleted)
            .AsQueryable();

        // Category filter: match slug on the product's own category OR its parent
        if (!string.IsNullOrWhiteSpace(filter.CategorySlug))
        {
            query = query.Where(p =>
                p.Category.Slug == filter.CategorySlug ||
                p.Category.Parent!.Slug == filter.CategorySlug);
        }

        // Text search
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(s) ||
                p.Brand.ToLower().Contains(s) ||
                p.Description.ToLower().Contains(s) ||
                (p.Tags != null && p.Tags.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Brand))
            query = query.Where(p => p.Brand.ToLower() == filter.Brand.ToLower());

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);

        if (filter.MinRating.HasValue)
            query = query.Where(p => p.AverageRating >= filter.MinRating.Value);

        if (filter.InStockOnly == true)
            query = query.Where(p => p.StockQuantity > 0);

        if (filter.FeaturedOnly == true)
            query = query.Where(p => p.IsFeatured);

        // Sorting
        query = filter.SortBy switch
        {
            "price_asc"  => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "rating"     => query.OrderByDescending(p => p.AverageRating),
            "popular"    => query.OrderByDescending(p => p.TotalSold),
            _            => query.OrderByDescending(p => p.CreatedAt)   // newest
        };

        var total = await query.CountAsync();
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);
        var page     = Math.Max(filter.Page, 1);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => ToListDto(p))
            .ToListAsync();

        return new PagedResult<ProductListDto>
        {
            Items      = items,
            TotalCount = total,
            PageNumber = page,
            PageSize   = pageSize
        };
    }

    public async Task<ProductDetailDto> GetBySlugAsync(string slug, Guid? userId = null)
    {
        var product = await _context.Products
            .Include(p => p.Category).ThenInclude(c => c.Parent)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Variants.Where(v => v.IsActive))
            .Include(p => p.Reviews.Where(r => !r.IsHidden).OrderByDescending(r => r.CreatedAt).Take(10))
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive && !p.IsDeleted)
            ?? throw new KeyNotFoundException($"Product '{slug}' not found.");

        // Rating breakdown
        var allRatings = await _context.Reviews
            .Where(r => r.ProductId == product.Id && !r.IsHidden)
            .GroupBy(r => r.Rating)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();

        var breakdown = new RatingBreakdownDto
        {
            FiveStar  = allRatings.FirstOrDefault(r => r.Key == 5)?.Count ?? 0,
            FourStar  = allRatings.FirstOrDefault(r => r.Key == 4)?.Count ?? 0,
            ThreeStar = allRatings.FirstOrDefault(r => r.Key == 3)?.Count ?? 0,
            TwoStar   = allRatings.FirstOrDefault(r => r.Key == 2)?.Count ?? 0,
            OneStar   = allRatings.FirstOrDefault(r => r.Key == 1)?.Count ?? 0,
        };

        // Related products: same sub-category, exclude this one
        var related = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == product.CategoryId
                     && p.Id != product.Id
                     && p.IsActive && !p.IsDeleted)
            .OrderByDescending(p => p.TotalSold)
            .Take(8)
            .Select(p => ToListDto(p))
            .ToListAsync();

        // Track recently viewed
        if (userId.HasValue)
            await TrackRecentlyViewedAsync(userId.Value, product.Id);

        var discount = CalcDiscount(product.Price, product.CompareAtPrice);

        return new ProductDetailDto
        {
            Id                  = product.Id,
            Name                = product.Name,
            Slug                = product.Slug,
            Brand               = product.Brand,
            Description         = product.Description,
            ShortDescription    = product.ShortDescription,
            Sku                 = product.Sku,
            Price               = product.Price,
            CompareAtPrice      = product.CompareAtPrice,
            DiscountPercent     = discount,
            PrimaryImageUrl     = product.ThumbnailUrl,
            StockQuantity       = product.StockQuantity,
            IsFeatured          = product.IsFeatured,
            AverageRating       = product.AverageRating,
            TotalReviews        = product.TotalReviews,
            TotalSold           = product.TotalSold,
            Weight              = product.Weight,
            Tags                = product.Tags,
            SpecificationsJson  = product.SpecificationsJson,
            CreatedAt           = product.CreatedAt,

            CategoryId          = product.CategoryId,
            CategoryName        = product.Category.Name,
            CategorySlug        = product.Category.Slug,
            ParentCategoryId    = product.Category.ParentId,
            ParentCategoryName  = product.Category.Parent?.Name,
            ParentCategorySlug  = product.Category.Parent?.Slug,

            Images = product.Images.Select(i => new ProductImageDto
            {
                Id        = i.Id,
                Url       = i.Url,
                AltText   = i.AltText,
                IsPrimary = i.IsPrimary,
                SortOrder = i.SortOrder
            }).ToList(),

            Variants = product.Variants.Select(v => new ProductVariantDto
            {
                Id            = v.Id,
                Name          = v.Name,
                Value         = v.Value,
                Type          = v.Type,
                PriceModifier = v.PriceModifier,
                StockQuantity = v.StockQuantity,
                Sku           = v.Sku
            }).ToList(),

            RecentReviews = product.Reviews.Select(r => new ReviewSummaryDto
            {
                Id               = r.Id,
                Rating           = r.Rating,
                Title            = r.Title,
                Body             = r.Body,
                ReviewerName     = $"{r.User.FirstName} {r.User.LastName[0]}.",
                IsVerifiedPurchase = r.IsVerifiedPurchase,
                HelpfulCount     = r.HelpfulCount,
                CreatedAt        = r.CreatedAt
            }).ToList(),

            RatingBreakdown = breakdown,
            RelatedProducts = related
        };
    }

    public async Task<List<ProductListDto>> GetFeaturedAsync(int count = 12)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsFeatured && p.IsActive && !p.IsDeleted && p.StockQuantity > 0)
            .OrderByDescending(p => p.TotalSold)
            .Take(count)
            .Select(p => ToListDto(p))
            .ToListAsync();
    }

    public async Task<List<ProductListDto>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => idList.Contains(p.Id) && p.IsActive && !p.IsDeleted)
            .Select(p => ToListDto(p))
            .ToListAsync();
    }

    public async Task<List<ProductSearchSuggestionDto>> SearchSuggestionsAsync(string query, int count = 8)
    {
        var q = query.ToLower();
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && !p.IsDeleted &&
                (p.Name.ToLower().Contains(q) || p.Brand.ToLower().Contains(q)))
            .OrderByDescending(p => p.TotalSold)
            .Take(count)
            .Select(p => new ProductSearchSuggestionDto
            {
                Id           = p.Id,
                Name         = p.Name,
                Slug         = p.Slug,
                PrimaryImageUrl = p.ThumbnailUrl,
                Price           = p.Price,
                CategoryName    = p.Category.Name
            })
            .ToListAsync();
    }

    public async Task<FilterMetaDto> GetFilterMetaAsync(string? categorySlug = null)
    {
        var query = _context.Products
            .Include(p => p.Category).ThenInclude(c => c.Parent)
            .Where(p => p.IsActive && !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(categorySlug))
        {
            query = query.Where(p =>
                p.Category.Slug == categorySlug ||
                p.Category.Parent!.Slug == categorySlug);
        }

        var brands   = await query.Select(p => p.Brand).Distinct().OrderBy(b => b).ToListAsync();
        var minPrice = await query.MinAsync(p => (decimal?)p.Price) ?? 0;
        var maxPrice = await query.MaxAsync(p => (decimal?)p.Price) ?? 0;
        var total    = await query.CountAsync();

        return new FilterMetaDto
        {
            Brands     = brands,
            MinPrice   = minPrice,
            MaxPrice   = maxPrice,
            TotalResults = total
        };
    }

    public async Task TrackRecentlyViewedAsync(Guid userId, Guid productId)
    {
        var existing = await _context.RecentlyViewed
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId);

        if (existing != null)
        {
            existing.ViewedAt = DateTime.UtcNow;
        }
        else
        {
            // Keep max 20 per user
            var count = await _context.RecentlyViewed.CountAsync(r => r.UserId == userId);
            if (count >= 20)
            {
                var oldest = await _context.RecentlyViewed
                    .Where(r => r.UserId == userId)
                    .OrderBy(r => r.ViewedAt)
                    .FirstAsync();
                _context.RecentlyViewed.Remove(oldest);
            }
            _context.RecentlyViewed.Add(new RecentlyViewed
            {
                UserId    = userId,
                ProductId = productId,
                ViewedAt  = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<ProductListDto>> GetRecentlyViewedAsync(Guid userId, int count = 10)
    {
        return await _context.RecentlyViewed
            .Include(r => r.Product).ThenInclude(p => p.Category)
            .Where(r => r.UserId == userId && r.Product.IsActive && !r.Product.IsDeleted)
            .OrderByDescending(r => r.ViewedAt)
            .Take(count)
            .Select(r => ToListDto(r.Product))
            .ToListAsync();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static ProductListDto ToListDto(Product p) => new()
    {
        Id               = p.Id,
        Name             = p.Name,
        Slug             = p.Slug,
        Brand            = p.Brand,
        ShortDescription = p.ShortDescription,
        Price            = p.Price,
        CompareAtPrice   = p.CompareAtPrice,
        DiscountPercent  = CalcDiscount(p.Price, p.CompareAtPrice),
        PrimaryImageUrl  = p.ThumbnailUrl,
        AverageRating    = p.AverageRating,
        TotalReviews     = p.TotalReviews,
        TotalSold        = p.TotalSold,
        Stock            = p.StockQuantity,
        IsFeatured       = p.IsFeatured,
        IsActive         = p.IsActive,
        Tags             = p.Tags,
        CategoryName     = p.Category?.Name ?? string.Empty,
        CategorySlug     = p.Category?.Slug ?? string.Empty
    };

    private static int? CalcDiscount(decimal price, decimal? compareAt)
    {
        if (!compareAt.HasValue || compareAt <= price) return null;
        return (int)Math.Round((compareAt.Value - price) / compareAt.Value * 100);
    }
}
