using ShopVerse.Domain.Common;

namespace ShopVerse.Domain.Entities;

public class RecentlyViewed : BaseEntity
{
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
