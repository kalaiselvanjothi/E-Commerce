namespace ShopVerse.Application.DTOs.Product;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
    public Guid? ParentId { get; set; }
    public int ProductCount { get; set; }
    public List<CategoryDto> Children { get; set; } = new();
}
