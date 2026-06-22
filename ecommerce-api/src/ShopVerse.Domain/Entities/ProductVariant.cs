using ShopVerse.Domain.Common;

namespace ShopVerse.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Type { get; set; }
    public decimal? PriceModifier { get; set; }
    public int StockQuantity { get; set; } = 0;
    public string? Sku { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
