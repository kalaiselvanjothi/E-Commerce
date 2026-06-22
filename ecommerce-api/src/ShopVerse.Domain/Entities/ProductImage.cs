using ShopVerse.Domain.Common;

namespace ShopVerse.Domain.Entities;

public class ProductImage : BaseEntity
{
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsPrimary { get; set; } = false;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
