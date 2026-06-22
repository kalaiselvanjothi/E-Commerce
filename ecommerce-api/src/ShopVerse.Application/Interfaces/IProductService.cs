using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.Product;

namespace ShopVerse.Application.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductListDto>> GetProductsAsync(ProductFilterDto filter);
    Task<ProductDetailDto> GetBySlugAsync(string slug, Guid? userId = null);
    Task<List<ProductListDto>> GetFeaturedAsync(int count = 12);
    Task<List<ProductListDto>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<List<ProductSearchSuggestionDto>> SearchSuggestionsAsync(string query, int count = 8);
    Task<FilterMetaDto> GetFilterMetaAsync(string? categorySlug = null);
    Task TrackRecentlyViewedAsync(Guid userId, Guid productId);
    Task<List<ProductListDto>> GetRecentlyViewedAsync(Guid userId, int count = 10);
}
