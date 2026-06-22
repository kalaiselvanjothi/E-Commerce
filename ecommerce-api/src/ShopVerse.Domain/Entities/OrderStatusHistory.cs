using ShopVerse.Domain.Common;
using ShopVerse.Domain.Enums;

namespace ShopVerse.Domain.Entities;

public class OrderStatusHistory : BaseEntity
{
    public OrderStatus Status { get; set; }
    public string? Comment { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
}
