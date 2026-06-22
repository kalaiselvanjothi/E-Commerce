using System.ComponentModel.DataAnnotations;

namespace ShopVerse.Application.DTOs.Product;

public class CreateReviewDto
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Body { get; set; }
}

public class ReviewDto
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string? ReviewerAvatar { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public bool IsHidden { get; set; }
    public int HelpfulCount { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
