using System.ComponentModel.DataAnnotations;

namespace ShopVerse.Application.DTOs.Admin;

public class CreateProductDto
{
    [Required][MaxLength(500)]  public string Name { get; set; } = string.Empty;
    [Required][MaxLength(200)]  public string Brand { get; set; } = string.Empty;
    [Required]                  public string Description { get; set; } = string.Empty;
    [MaxLength(500)]            public string? ShortDescription { get; set; }
    [Required][MaxLength(100)]  public string Sku { get; set; } = string.Empty;
    [Required][Range(0, double.MaxValue)] public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? CostPrice { get; set; }
    [Range(0, int.MaxValue)]    public int Stock { get; set; }
    [Required]                  public Guid CategoryId { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? SpecificationsJson { get; set; }
    public string? Tags { get; set; }
    public decimal Weight { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; } = false;
    public bool TrackInventory { get; set; } = true;
    public List<ProductImageInputDto> Images { get; set; } = new();
    public List<ProductVariantInputDto> Variants { get; set; } = new();
}

public class UpdateProductDto : CreateProductDto { }

public class ProductImageInputDto
{
    [Required] public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}

public class ProductVariantInputDto
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Value { get; set; } = string.Empty;
    public string? Type { get; set; }
    public decimal? PriceModifier { get; set; }
    public int Stock { get; set; }
    public string? Sku { get; set; }
}

public class AdminProductListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalSold { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
