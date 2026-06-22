using ShopVerse.Application.DTOs.Product;

namespace ShopVerse.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<List<CategoryDto>> GetTopLevelAsync();
    Task<CategoryDto> GetBySlugAsync(string slug);
}
