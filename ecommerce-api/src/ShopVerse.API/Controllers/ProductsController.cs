using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.Product;
using ShopVerse.Application.Interfaces;

namespace ShopVerse.API.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
        => _productService = productService;

    /// <summary>
    /// List products with filters, search and pagination.
    /// Query params: search, categorySlug, brand, minPrice, maxPrice,
    ///               minRating, inStockOnly, featuredOnly, sortBy, page, pageSize
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ProductListDto>>), 200)]
    public async Task<IActionResult> GetProducts([FromQuery] ProductFilterDto filter)
    {
        var result = await _productService.GetProductsAsync(filter);
        return Ok(ApiResponse<PagedResult<ProductListDto>>.Ok(result));
    }

    /// <summary>Get featured products for homepage hero / carousel</summary>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(ApiResponse<List<ProductListDto>>), 200)]
    public async Task<IActionResult> GetFeatured([FromQuery] int count = 12)
    {
        var result = await _productService.GetFeaturedAsync(count);
        return Ok(ApiResponse<List<ProductListDto>>.Ok(result));
    }

    /// <summary>Autocomplete search suggestions (max 8)</summary>
    [HttpGet("search/suggestions")]
    [ProducesResponseType(typeof(ApiResponse<List<ProductSearchSuggestionDto>>), 200)]
    public async Task<IActionResult> SearchSuggestions([FromQuery] string q, [FromQuery] int count = 8)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Ok(ApiResponse<List<ProductSearchSuggestionDto>>.Ok(new()));

        var suggestions = await _productService.SearchSuggestionsAsync(q, count);
        return Ok(ApiResponse<List<ProductSearchSuggestionDto>>.Ok(suggestions));
    }

    /// <summary>Filter metadata (brands list, price range) for a category or all products</summary>
    [HttpGet("filter-meta")]
    [ProducesResponseType(typeof(ApiResponse<FilterMetaDto>), 200)]
    public async Task<IActionResult> GetFilterMeta([FromQuery] string? categorySlug = null)
    {
        var meta = await _productService.GetFilterMetaAsync(categorySlug);
        return Ok(ApiResponse<FilterMetaDto>.Ok(meta));
    }

    /// <summary>Get recently viewed products for the current user</summary>
    [HttpGet("recently-viewed")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<ProductListDto>>), 200)]
    public async Task<IActionResult> GetRecentlyViewed([FromQuery] int count = 10)
    {
        var userId = GetUserId();
        var result = await _productService.GetRecentlyViewedAsync(userId, count);
        return Ok(ApiResponse<List<ProductListDto>>.Ok(result));
    }

    /// <summary>Get product detail by slug (tracks recently viewed if authenticated)</summary>
    [HttpGet("{slug}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDetailDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var userId = TryGetUserId();
        var product = await _productService.GetBySlugAsync(slug, userId);
        return Ok(ApiResponse<ProductDetailDto>.Ok(product));
    }

    private Guid? TryGetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return claim != null ? Guid.Parse(claim) : null;
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
