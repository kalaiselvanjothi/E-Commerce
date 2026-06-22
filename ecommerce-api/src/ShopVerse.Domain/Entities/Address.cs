using ShopVerse.Domain.Common;

namespace ShopVerse.Domain.Entities;

public class Address : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "India";
    public bool IsDefault { get; set; } = false;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
