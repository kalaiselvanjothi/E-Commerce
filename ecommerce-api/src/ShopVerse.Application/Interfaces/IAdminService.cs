using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.Admin;

namespace ShopVerse.Application.Interfaces;

public interface IAdminService
{
    Task<DashboardStatsDto> GetDashboardAsync();
    // Products
    Task<PagedResult<AdminProductListDto>> GetProductsAsync(int page, int pageSize, string? search, bool? isActive);
    Task<AdminProductListDto> CreateProductAsync(CreateProductDto dto);
    Task<AdminProductListDto> UpdateProductAsync(Guid id, UpdateProductDto dto);
    Task DeleteProductAsync(Guid id);
    Task ToggleProductActiveAsync(Guid id);
    // Categories
    Task<CategoryAdminDto> CreateCategoryAsync(CreateCategoryDto dto);
    Task<CategoryAdminDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto);
    Task DeleteCategoryAsync(Guid id);
    // Customers
    Task<PagedResult<CustomerAdminDto>> GetCustomersAsync(int page, int pageSize, string? search);
    Task ToggleCustomerActiveAsync(Guid id);
    // Coupons
    Task<List<CouponAdminDto>> GetCouponsAsync();
    Task<CouponAdminDto> CreateCouponAsync(CreateCouponDto dto);
    Task<CouponAdminDto> UpdateCouponAsync(Guid id, CreateCouponDto dto);
    Task DeleteCouponAsync(Guid id);
    Task ToggleCouponActiveAsync(Guid id);
}
