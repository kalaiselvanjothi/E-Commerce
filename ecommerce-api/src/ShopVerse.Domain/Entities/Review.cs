using ShopVerse.Domain.Common;

namespace ShopVerse.Domain.Entities;

public class Review : BaseEntity
{
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public bool IsVerifiedPurchase { get; set; } = false;
    public bool IsHidden { get; set; } = false;
    public int HelpfulCount { get; set; } = 0;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
