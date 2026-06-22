using System.ComponentModel.DataAnnotations;
using ShopVerse.Domain.Enums;

namespace ShopVerse.Application.DTOs.Admin;

public class CreateCouponDto
{
    [Required][MaxLength(50)] public string Code { get; set; } = string.Empty;
    [MaxLength(300)]          public string? Description { get; set; }
    [Required]                public CouponType Type { get; set; }
    [Required][Range(0, double.MaxValue)] public decimal Value { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int? PerUserLimit { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class CouponAdminDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CouponType Type { get; set; }
    public string TypeLabel { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public int? PerUserLimit { get; set; }
    public bool IsActive { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;
    public DateTime CreatedAt { get; set; }
}
