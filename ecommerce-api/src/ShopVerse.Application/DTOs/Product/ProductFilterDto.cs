namespace ShopVerse.Application.DTOs.Product;

public class ProductFilterDto
{
    public string? Search { get; set; }
    public string? CategorySlug { get; set; }
    public string? Brand { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public double? MinRating { get; set; }
    public bool? InStockOnly { get; set; }
    public bool? FeaturedOnly { get; set; }
    public string SortBy { get; set; } = "newest";   // newest | price_asc | price_desc | rating | popular
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ProductSearchSuggestionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? PrimaryImageUrl { get; set; }
    public decimal Price { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class FilterMetaDto
{
    public List<string> Brands { get; set; } = new();
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public int TotalResults { get; set; }
}
