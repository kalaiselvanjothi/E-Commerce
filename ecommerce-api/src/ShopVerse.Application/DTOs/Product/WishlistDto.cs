namespace ShopVerse.Application.DTOs.Product;

public class WishlistDto
{
    public Guid Id { get; set; }
    public List<WishlistItemDto> Items { get; set; } = new();
    public int ItemCount { get; set; }
}

public class WishlistItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int? DiscountPercent { get; set; }
    public int Stock { get; set; }
    public double AverageRating { get; set; }
    public DateTime AddedAt { get; set; }
}

public class WishlistAddDto
{
    public Guid ProductId { get; set; }
}
