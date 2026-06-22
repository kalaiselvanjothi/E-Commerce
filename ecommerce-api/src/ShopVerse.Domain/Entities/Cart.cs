using ShopVerse.Domain.Common;

namespace ShopVerse.Domain.Entities;

public class Cart : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string? CouponCode { get; set; }
    public decimal DiscountAmount { get; set; } = 0;

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
