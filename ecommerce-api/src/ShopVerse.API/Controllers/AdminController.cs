using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.Admin;
using ShopVerse.Application.Interfaces;

namespace ShopVerse.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    public AdminController(IAdminService adminService) => _adminService = adminService;

    // ─── Dashboard ────────────────────────────────────────────────────────────

    /// <summary>[Admin] Sales analytics dashboard — revenue, orders, top products</summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardStatsDto>), 200)]
    public async Task<IActionResult> GetDashboard()
    {
        var stats = await _adminService.GetDashboardAsync();
        return Ok(ApiResponse<DashboardStatsDto>.Ok(stats));
    }

    // ─── Products ─────────────────────────────────────────────────────────────

    /// <summary>[Admin] List all products with search and active filter</summary>
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _adminService.GetProductsAsync(page, pageSize, search, isActive);
        return Ok(ApiResponse<PagedResult<AdminProductListDto>>.Ok(result));
    }

    /// <summary>[Admin] Create a new product with images and variants</summary>
    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var product = await _adminService.CreateProductAsync(dto);
        return CreatedAtAction(nameof(GetProducts), ApiResponse<AdminProductListDto>.Ok(product, "Product created"));
    }

    /// <summary>[Admin] Update an existing product</summary>
    [HttpPut("products/{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        var product = await _adminService.UpdateProductAsync(id, dto);
        return Ok(ApiResponse<AdminProductListDto>.Ok(product, "Product updated"));
    }

    /// <summary>[Admin] Soft-delete a product</summary>
    [HttpDelete("products/{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await _adminService.DeleteProductAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, "Product deleted"));
    }

    /// <summary>[Admin] Toggle product active/inactive</summary>
    [HttpPatch("products/{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleProductActive(Guid id)
    {
        await _adminService.ToggleProductActiveAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, "Product visibility toggled"));
    }

    // ─── Categories ───────────────────────────────────────────────────────────

    /// <summary>[Admin] Create a category (supports parent for subcategories)</summary>
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var cat = await _adminService.CreateCategoryAsync(dto);
        return CreatedAtAction(nameof(GetDashboard), ApiResponse<CategoryAdminDto>.Ok(cat, "Category created"));
    }

    /// <summary>[Admin] Update an existing category</summary>
    [HttpPut("categories/{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        var cat = await _adminService.UpdateCategoryAsync(id, dto);
        return Ok(ApiResponse<CategoryAdminDto>.Ok(cat, "Category updated"));
    }

    /// <summary>[Admin] Delete a category (fails if it has products)</summary>
    [HttpDelete("categories/{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        await _adminService.DeleteCategoryAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, "Category deleted"));
    }

    // ─── Customers ────────────────────────────────────────────────────────────

    /// <summary>[Admin] List all customers with order stats</summary>
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null)
    {
        var result = await _adminService.GetCustomersAsync(page, pageSize, search);
        return Ok(ApiResponse<PagedResult<CustomerAdminDto>>.Ok(result));
    }

    /// <summary>[Admin] Activate or deactivate a customer account</summary>
    [HttpPatch("customers/{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleCustomerActive(Guid id)
    {
        await _adminService.ToggleCustomerActiveAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, "Customer status toggled"));
    }

    // ─── Coupons ──────────────────────────────────────────────────────────────

    /// <summary>[Admin] List all coupons</summary>
    [HttpGet("coupons")]
    public async Task<IActionResult> GetCoupons()
    {
        var coupons = await _adminService.GetCouponsAsync();
        return Ok(ApiResponse<List<CouponAdminDto>>.Ok(coupons));
    }

    /// <summary>[Admin] Create a new coupon</summary>
    [HttpPost("coupons")]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponDto dto)
    {
        var coupon = await _adminService.CreateCouponAsync(dto);
        return CreatedAtAction(nameof(GetCoupons), ApiResponse<CouponAdminDto>.Ok(coupon, "Coupon created"));
    }

    /// <summary>[Admin] Update coupon details</summary>
    [HttpPut("coupons/{id:guid}")]
    public async Task<IActionResult> UpdateCoupon(Guid id, [FromBody] CreateCouponDto dto)
    {
        var coupon = await _adminService.UpdateCouponAsync(id, dto);
        return Ok(ApiResponse<CouponAdminDto>.Ok(coupon, "Coupon updated"));
    }

    /// <summary>[Admin] Delete a coupon</summary>
    [HttpDelete("coupons/{id:guid}")]
    public async Task<IActionResult> DeleteCoupon(Guid id)
    {
        await _adminService.DeleteCouponAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, "Coupon deleted"));
    }

    /// <summary>[Admin] Toggle coupon active/inactive</summary>
    [HttpPatch("coupons/{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleCouponActive(Guid id)
    {
        await _adminService.ToggleCouponActiveAsync(id);
        return Ok(ApiResponse<object>.Ok(null!, "Coupon status toggled"));
    }
}
