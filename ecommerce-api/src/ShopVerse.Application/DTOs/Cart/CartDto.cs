using System.ComponentModel.DataAnnotations;

namespace ShopVerse.Application.DTOs.Cart;

public class CartDto
{
    public Guid Id { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public string? CouponCode { get; set; }
    public decimal? CouponDiscount { get; set; }
    public int ItemCount { get; set; }
    public bool IsFreeShipping { get; set; }
}

public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public string Brand { get; set; } = string.Empty;
    public Guid? VariantId { get; set; }
    public string? VariantName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int Stock { get; set; }
}

public class AddToCartDto
{
    [Required] public Guid ProductId { get; set; }
    [Range(1, 100)] public int Quantity { get; set; } = 1;
    public Guid? VariantId { get; set; }
}

public class UpdateCartItemDto
{
    [Required][Range(0, 100)] public int Quantity { get; set; }
}

public class ApplyCouponDto
{
    [Required][MaxLength(50)] public string Code { get; set; } = string.Empty;
}
