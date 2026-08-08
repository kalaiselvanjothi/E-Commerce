using ShopVerse.Domain.Common;

namespace ShopVerse.Domain.Entities;

public class ContactEnquiry : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? OrderNumber { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsResolved { get; set; } = false;
}
