using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.Admin;
using ShopVerse.Application.DTOs.Product;
using ShopVerse.Application.Interfaces;
using ShopVerse.Domain.Entities;
using ShopVerse.Domain.Enums;
using ShopVerse.Infrastructure.Data;

namespace ShopVerse.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly ShopVerseDbContext _context;
    private readonly UserManager<User> _userManager;

    public AdminService(ShopVerseDbContext context, UserManager<User> userManager)
    {
        _context     = context;
        _userManager = userManager;
    }

    // ─── Dashboard ────────────────────────────────────────────────────────────

    public async Task<DashboardStatsDto> GetDashboardAsync()
    {
        var now     = DateTime.UtcNow;
        var last30  = now.AddDays(-30);
        var prev30  = now.AddDays(-60);

        var orders = await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.User)
            .ToListAsync();

        var deliveredOrders = orders.Where(o => o.Status == OrderStatus.Delivered).ToList();

        // Revenue
        var totalRevenue    = deliveredOrders.Sum(o => o.TotalAmount);
        var rev30           = deliveredOrders.Where(o => o.CreatedAt >= last30).Sum(o => o.TotalAmount);
        var revPrev         = deliveredOrders.Where(o => o.CreatedAt >= prev30 && o.CreatedAt < last30).Sum(o => o.TotalAmount);
        var revChange       = revPrev > 0 ? Math.Round((double)(rev30 - revPrev) / (double)revPrev * 100, 1) : 100;

        // Orders
        var orders30        = orders.Where(o => o.CreatedAt >= last30).ToList();
        var pending         = orders.Count(o => o.Status == OrderStatus.Placed);
        var processing      = orders.Count(o => o.Status is OrderStatus.Confirmed or OrderStatus.Packed or OrderStatus.Shipped or OrderStatus.OutForDelivery);
        var delivered       = orders.Count(o => o.Status == OrderStatus.Delivered);
        var cancelled       = orders.Count(o => o.Status == OrderStatus.Cancelled);

        // Customers
        var users       = await _userManager.GetUsersInRoleAsync("Customer");
        var newUsers30  = users.Count(u => u.CreatedAt >= last30);
        var activeUsers = users.Count(u => u.IsActive);

        // Products
        var products    = await _context.Products.Where(p => !p.IsDeleted).ToListAsync();
        var outOfStock  = products.Count(p => p.StockQuantity == 0);
        var lowStock    = products.Count(p => p.StockQuantity > 0 && p.StockQuantity < 10);

        // Charts: last 30 days by day
        var revenueChart = Enumerable.Range(0, 30).Select(i =>
        {
            var date  = now.AddDays(-29 + i).Date;
            var value = deliveredOrders
                .Where(o => o.CreatedAt.Date == date)
                .Sum(o => o.TotalAmount);
            return new ChartPointDto { Date = date.ToString("MMM dd"), Value = value };
        }).ToList();

        var ordersChart = Enumerable.Range(0, 30).Select(i =>
        {
            var date  = now.AddDays(-29 + i).Date;
            var count = orders.Count(o => o.CreatedAt.Date == date);
            return new ChartPointDto { Date = date.ToString("MMM dd"), Value = count };
        }).ToList();

        // Top products
        var topProducts = products
            .OrderByDescending(p => p.TotalSold)
            .Take(5)
            .Select(p => new TopProductDto
            {
                Id        = p.Id,
                Name      = p.Name,
                ImageUrl  = p.ThumbnailUrl,
                TotalSold = p.TotalSold,
                Revenue   = orders.SelectMany(o => o.Items)
                                  .Where(i => i.ProductId == p.Id)
                                  .Sum(i => i.TotalPrice)
            })
            .ToList();

        // Recent orders
        var recentOrders = orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .Select(o => new RecentOrderDto
            {
                Id           = o.Id,
                OrderNumber  = o.OrderNumber,
                CustomerName = $"{o.User.FirstName} {o.User.LastName}",
                Total        = o.TotalAmount,
                Status       = o.Status.ToString(),
                CreatedAt    = o.CreatedAt
            })
            .ToList();

        return new DashboardStatsDto
        {
            TotalRevenue    = totalRevenue,
            RevenueChange   = revChange,
            TotalOrders     = orders.Count,
            OrdersChange    = orders30.Count > 0 ? Math.Round((double)orders30.Count / orders.Count * 100, 1) : 0,
            TotalCustomers  = users.Count,
            CustomersChange = newUsers30,
            TotalProducts   = products.Count,
            PendingOrders   = pending,
            RevenueChart    = revenueChart,
            OrdersChart     = ordersChart,
            TopProducts     = topProducts,
            RecentOrders    = recentOrders
        };
    }

    // ─── Products ─────────────────────────────────────────────────────────────

    public async Task<PagedResult<AdminProductListDto>> GetProductsAsync(
        int page, int pageSize, string? search, bool? isActive)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s)
                                  || p.Brand.ToLower().Contains(s)
                                  || p.Sku.ToLower().Contains(s));
        }

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        query = query.OrderByDescending(p => p.CreatedAt);
        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => ToAdminListDto(p))
            .ToListAsync();

        return new PagedResult<AdminProductListDto>
        {
            Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize
        };
    }

    public async Task<AdminProductListDto> CreateProductAsync(CreateProductDto dto)
    {
        if (await _context.Products.AnyAsync(p => p.Sku == dto.Sku && !p.IsDeleted))
            throw new InvalidOperationException($"SKU '{dto.Sku}' already exists.");

        var slug = await GenerateUniqueSlugAsync(dto.Name);

        var product = new Product
        {
            Name               = dto.Name,
            Slug               = slug,
            Brand              = dto.Brand,
            Description        = dto.Description,
            ShortDescription   = dto.ShortDescription,
            Sku                = dto.Sku,
            Price              = dto.Price,
            CompareAtPrice     = dto.CompareAtPrice,
            CostPrice          = dto.CostPrice,
            StockQuantity      = dto.Stock,
            CategoryId         = dto.CategoryId,
            ThumbnailUrl       = dto.ThumbnailUrl,
            SpecificationsJson = dto.SpecificationsJson,
            Tags               = dto.Tags,
            Weight             = dto.Weight,
            IsActive           = dto.IsActive,
            IsFeatured         = dto.IsFeatured,
            TrackInventory     = dto.TrackInventory,
            Images = dto.Images.Select((img, idx) => new ProductImage
            {
                Url       = img.Url,
                AltText   = img.AltText,
                SortOrder = img.SortOrder > 0 ? img.SortOrder : idx,
                IsPrimary = img.IsPrimary || idx == 0
            }).ToList(),
            Variants = dto.Variants.Select(v => new ProductVariant
            {
                Name          = v.Name,
                Value         = v.Value,
                Type          = v.Type,
                PriceModifier = v.PriceModifier,
                StockQuantity = v.Stock,
                Sku           = v.Sku,
                IsActive      = true
            }).ToList()
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        await _context.Entry(product).Reference(p => p.Category).LoadAsync();
        return ToAdminListDto(product);
    }

    public async Task<AdminProductListDto> UpdateProductAsync(Guid id, UpdateProductDto dto)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted)
            ?? throw new KeyNotFoundException("Product not found.");

        if (await _context.Products.AnyAsync(p => p.Sku == dto.Sku && p.Id != id && !p.IsDeleted))
            throw new InvalidOperationException($"SKU '{dto.Sku}' already used by another product.");

        // Regenerate slug if name changed
        if (product.Name != dto.Name)
            product.Slug = await GenerateUniqueSlugAsync(dto.Name, id);

        product.Name               = dto.Name;
        product.Brand              = dto.Brand;
        product.Description        = dto.Description;
        product.ShortDescription   = dto.ShortDescription;
        product.Sku                = dto.Sku;
        product.Price              = dto.Price;
        product.CompareAtPrice     = dto.CompareAtPrice;
        product.CostPrice          = dto.CostPrice;
        product.StockQuantity      = dto.Stock;
        product.CategoryId         = dto.CategoryId;
        product.ThumbnailUrl       = dto.ThumbnailUrl;
        product.SpecificationsJson = dto.SpecificationsJson;
        product.Tags               = dto.Tags;
        product.Weight             = dto.Weight;
        product.IsActive           = dto.IsActive;
        product.IsFeatured         = dto.IsFeatured;
        product.TrackInventory     = dto.TrackInventory;
        product.UpdatedAt          = DateTime.UtcNow;

        // Replace images
        _context.ProductImages.RemoveRange(product.Images);
        product.Images = dto.Images.Select((img, idx) => new ProductImage
        {
            ProductId = product.Id,
            Url       = img.Url,
            AltText   = img.AltText,
            SortOrder = img.SortOrder > 0 ? img.SortOrder : idx,
            IsPrimary = img.IsPrimary || idx == 0
        }).ToList();

        // Replace variants
        _context.ProductVariants.RemoveRange(product.Variants);
        product.Variants = dto.Variants.Select(v => new ProductVariant
        {
            ProductId     = product.Id,
            Name          = v.Name,
            Value         = v.Value,
            Type          = v.Type,
            PriceModifier = v.PriceModifier,
            StockQuantity = v.Stock,
            Sku           = v.Sku,
            IsActive      = true
        }).ToList();

        await _context.SaveChangesAsync();
        return ToAdminListDto(product);
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id)
            ?? throw new KeyNotFoundException("Product not found.");
        product.IsDeleted  = true;
        product.IsActive   = false;
        product.UpdatedAt  = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task ToggleProductActiveAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id)
            ?? throw new KeyNotFoundException("Product not found.");
        product.IsActive  = !product.IsActive;
        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    // ─── Categories ───────────────────────────────────────────────────────────

    public async Task<CategoryAdminDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var slug = await GenerateCategorySlugAsync(dto.Name);
        var category = new Category
        {
            Name        = dto.Name,
            Slug        = slug,
            Description = dto.Description,
            ImageUrl    = dto.ImageUrl,
            ParentId    = dto.ParentId,
            SortOrder   = dto.SortOrder,
            IsActive    = dto.IsActive
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return ToCategoryAdminDto(category);
    }

    public async Task<CategoryAdminDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException("Category not found.");

        if (category.Name != dto.Name)
            category.Slug = await GenerateCategorySlugAsync(dto.Name, id);

        category.Name        = dto.Name;
        category.Description = dto.Description;
        category.ImageUrl    = dto.ImageUrl;
        category.ParentId    = dto.ParentId;
        category.SortOrder   = dto.SortOrder;
        category.IsActive    = dto.IsActive;
        category.UpdatedAt   = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ToCategoryAdminDto(category);
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id && !p.IsDeleted);
        if (hasProducts)
            throw new InvalidOperationException("Cannot delete a category that has products.");

        var category = await _context.Categories.FindAsync(id)
            ?? throw new KeyNotFoundException("Category not found.");
        category.IsDeleted = true;
        category.IsActive  = false;
        await _context.SaveChangesAsync();
    }

    // ─── Customers ────────────────────────────────────────────────────────────

    public async Task<PagedResult<CustomerAdminDto>> GetCustomersAsync(int page, int pageSize, string? search)
    {
        var customers = await _userManager.GetUsersInRoleAsync("Customer");
        IEnumerable<User> query = customers;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(u =>
                u.Email!.Contains(s) ||
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s));
        }

        var ordered = query.OrderByDescending(u => u.CreatedAt).ToList();
        var total   = ordered.Count;
        var paged   = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var userIds = paged.Select(u => u.Id).ToList();
        var orderStats = await _context.Orders
            .Where(o => userIds.Contains(o.UserId))
            .GroupBy(o => o.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count(), Spent = g.Sum(o => o.TotalAmount) })
            .ToDictionaryAsync(x => x.UserId);

        var items = paged.Select(u => new CustomerAdminDto
        {
            Id          = u.Id,
            FirstName   = u.FirstName,
            LastName    = u.LastName,
            Email       = u.Email!,
            PhoneNumber = u.PhoneNumber,
            IsActive    = u.IsActive,
            CreatedAt   = u.CreatedAt,
            TotalOrders = orderStats.ContainsKey(u.Id) ? orderStats[u.Id].Count : 0,
            TotalSpent  = orderStats.ContainsKey(u.Id) ? orderStats[u.Id].Spent : 0
        }).ToList();

        return new PagedResult<CustomerAdminDto>
        {
            Items = items, TotalCount = total, PageNumber = page, PageSize = pageSize
        };
    }

    public async Task ToggleCustomerActiveAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString())
            ?? throw new KeyNotFoundException("Customer not found.");
        user.IsActive  = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
    }

    // ─── Coupons ──────────────────────────────────────────────────────────────

    public async Task<List<CouponAdminDto>> GetCouponsAsync()
    {
        return await _context.Coupons
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => ToCouponAdminDto(c))
            .ToListAsync();
    }

    public async Task<CouponAdminDto> CreateCouponAsync(CreateCouponDto dto)
    {
        var code = dto.Code.ToUpper().Trim();
        if (await _context.Coupons.AnyAsync(c => c.Code == code && !c.IsDeleted))
            throw new InvalidOperationException($"Coupon code '{code}' already exists.");

        var coupon = new Coupon
        {
            Code              = code,
            Description       = dto.Description,
            Type              = dto.Type,
            Value             = dto.Value,
            MinOrderAmount    = dto.MinOrderAmount,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            UsageLimit        = dto.UsageLimit,
            PerUserLimit      = dto.PerUserLimit,
            IsActive          = dto.IsActive,
            StartsAt          = dto.StartsAt,
            ExpiresAt         = dto.ExpiresAt
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();
        return ToCouponAdminDto(coupon);
    }

    public async Task<CouponAdminDto> UpdateCouponAsync(Guid id, CreateCouponDto dto)
    {
        var coupon = await _context.Coupons.FindAsync(id)
            ?? throw new KeyNotFoundException("Coupon not found.");

        var code = dto.Code.ToUpper().Trim();
        if (await _context.Coupons.AnyAsync(c => c.Code == code && c.Id != id && !c.IsDeleted))
            throw new InvalidOperationException($"Coupon code '{code}' already used.");

        coupon.Code              = code;
        coupon.Description       = dto.Description;
        coupon.Type              = dto.Type;
        coupon.Value             = dto.Value;
        coupon.MinOrderAmount    = dto.MinOrderAmount;
        coupon.MaxDiscountAmount = dto.MaxDiscountAmount;
        coupon.UsageLimit        = dto.UsageLimit;
        coupon.PerUserLimit      = dto.PerUserLimit;
        coupon.IsActive          = dto.IsActive;
        coupon.StartsAt          = dto.StartsAt;
        coupon.ExpiresAt         = dto.ExpiresAt;
        coupon.UpdatedAt         = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ToCouponAdminDto(coupon);
    }

    public async Task DeleteCouponAsync(Guid id)
    {
        var coupon = await _context.Coupons.FindAsync(id)
            ?? throw new KeyNotFoundException("Coupon not found.");
        coupon.IsDeleted = true;
        coupon.IsActive  = false;
        await _context.SaveChangesAsync();
    }

    public async Task ToggleCouponActiveAsync(Guid id)
    {
        var coupon = await _context.Coupons.FindAsync(id)
            ?? throw new KeyNotFoundException("Coupon not found.");
        coupon.IsActive  = !coupon.IsActive;
        coupon.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<string> GenerateUniqueSlugAsync(string name, Guid? excludeId = null)
    {
        var base64 = name.ToLower()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("/", "-")
            .Replace("\\", "-");
        var slug = System.Text.RegularExpressions.Regex.Replace(base64, @"[^a-z0-9\-]", "");

        var candidate = slug;
        var counter   = 1;
        while (await _context.Products.AnyAsync(p =>
            p.Slug == candidate && !p.IsDeleted && (excludeId == null || p.Id != excludeId)))
        {
            candidate = $"{slug}-{counter++}";
        }
        return candidate;
    }

    private async Task<string> GenerateCategorySlugAsync(string name, Guid? excludeId = null)
    {
        var base64 = name.ToLower().Replace(" ", "-").Replace("'", "").Replace("&", "and");
        var slug   = System.Text.RegularExpressions.Regex.Replace(base64, @"[^a-z0-9\-]", "");

        var candidate = slug;
        var counter   = 1;
        while (await _context.Categories.AnyAsync(c =>
            c.Slug == candidate && !c.IsDeleted && (excludeId == null || c.Id != excludeId)))
        {
            candidate = $"{slug}-{counter++}";
        }
        return candidate;
    }

    private static AdminProductListDto ToAdminListDto(Product p) => new()
    {
        Id              = p.Id,
        Name            = p.Name,
        Slug            = p.Slug,
        Brand           = p.Brand,
        Sku             = p.Sku,
        Price           = p.Price,
        Stock           = p.StockQuantity,
        IsActive        = p.IsActive,
        IsFeatured      = p.IsFeatured,
        CategoryName    = p.Category?.Name ?? string.Empty,
        AverageRating   = p.AverageRating,
        TotalSold       = p.TotalSold,
        PrimaryImageUrl = p.ThumbnailUrl,
        CreatedAt       = p.CreatedAt
    };

    private static CategoryAdminDto ToCategoryAdminDto(Category c) => new()
    {
        Id          = c.Id,
        Name        = c.Name,
        Slug        = c.Slug,
        Description = c.Description,
        ImageUrl    = c.ImageUrl,
        ParentId    = c.ParentId,
        SortOrder   = c.SortOrder,
        Children    = new()
    };

    private static CouponAdminDto ToCouponAdminDto(Coupon c) => new()
    {
        Id                = c.Id,
        Code              = c.Code,
        Description       = c.Description,
        Type              = c.Type,
        TypeLabel         = c.Type switch
        {
            CouponType.Percentage   => "Percentage",
            CouponType.FixedAmount  => "Fixed Amount",
            CouponType.FreeShipping => "Free Shipping",
            _                       => c.Type.ToString()
        },
        Value             = c.Value,
        MinOrderAmount    = c.MinOrderAmount,
        MaxDiscountAmount = c.MaxDiscountAmount,
        UsageLimit        = c.UsageLimit,
        UsedCount         = c.UsedCount,
        PerUserLimit      = c.PerUserLimit,
        IsActive          = c.IsActive,
        StartsAt          = c.StartsAt,
        ExpiresAt         = c.ExpiresAt,
        CreatedAt         = c.CreatedAt
    };
}
