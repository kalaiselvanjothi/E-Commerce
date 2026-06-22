using System.ComponentModel.DataAnnotations;

namespace ShopVerse.Application.DTOs.User;

public class AddressDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string AddressType { get; set; } = "Home";
}

public class CreateAddressDto
{
    [Required] [MaxLength(200)] public string FullName { get; set; } = string.Empty;
    [Required] [Phone]          public string Phone { get; set; } = string.Empty;
    [Required] [MaxLength(500)] public string AddressLine1 { get; set; } = string.Empty;
    [MaxLength(500)]            public string? AddressLine2 { get; set; }
    [Required] [MaxLength(100)] public string City { get; set; } = string.Empty;
    [Required] [MaxLength(100)] public string State { get; set; } = string.Empty;
    [Required] [MaxLength(20)]  public string Pincode { get; set; } = string.Empty;
    [MaxLength(100)]            public string Country { get; set; } = "India";
    public bool IsDefault { get; set; } = false;
    [MaxLength(50)]             public string AddressType { get; set; } = "Home";
}
