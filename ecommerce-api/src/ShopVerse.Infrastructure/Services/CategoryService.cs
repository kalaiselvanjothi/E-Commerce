using Microsoft.EntityFrameworkCore;
using ShopVerse.Application.DTOs.Product;
using ShopVerse.Application.Interfaces;
using ShopVerse.Domain.Entities;
using ShopVerse.Infrastructure.Data;

namespace ShopVerse.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly ShopVerseDbContext _context;

    public CategoryService(ShopVerseDbContext context) => _context = context;

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var all = await _context.Categories
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        var productCounts = await _context.Products
            .Where(p => p.IsActive && !p.IsDeleted)
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

        return BuildTree(all, null, productCounts);
    }

    public async Task<List<CategoryDto>> GetTopLevelAsync()
    {
        var all = await _context.Categories
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        var productCounts = await _context.Products
            .Where(p => p.IsActive && !p.IsDeleted)
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

        return BuildTree(all, null, productCounts);
    }

    public async Task<CategoryDto> GetBySlugAsync(string slug)
    {
        var category = await _context.Categories
            .Include(c => c.Children.Where(ch => ch.IsActive && !ch.IsDeleted))
            .Include(c => c.Parent)
            .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive && !c.IsDeleted)
            ?? throw new KeyNotFoundException($"Category '{slug}' not found.");

        var productCounts = await _context.Products
            .Where(p => p.IsActive && !p.IsDeleted)
            .GroupBy(p => p.CategoryId)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

        return MapDto(category, productCounts);
    }

    private static List<CategoryDto> BuildTree(
        List<Category> all,
        Guid? parentId,
        Dictionary<Guid, int> counts)
    {
        return all
            .Where(c => c.ParentId == parentId)
            .OrderBy(c => c.SortOrder)
            .Select(c =>
            {
                var dto = MapDto(c, counts);
                dto.Children = BuildTree(all, c.Id, counts);
                dto.ProductCount += dto.Children.Sum(ch => ch.ProductCount);
                return dto;
            })
            .ToList();
    }

    private static CategoryDto MapDto(Category c, Dictionary<Guid, int> counts) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Slug = c.Slug,
        Description = c.Description,
        ImageUrl = c.ImageUrl,
        SortOrder = c.SortOrder,
        ParentId = c.ParentId,
        ProductCount = counts.GetValueOrDefault(c.Id, 0)
    };
}
