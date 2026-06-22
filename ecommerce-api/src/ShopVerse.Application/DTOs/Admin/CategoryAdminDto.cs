using System.ComponentModel.DataAnnotations;

namespace ShopVerse.Application.DTOs.Admin;

public class CategoryAdminDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public List<CategoryAdminDto> Children { get; set; } = new();
}

public class CreateCategoryDto
{
    [Required][MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)]           public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateCategoryDto : CreateCategoryDto { }
