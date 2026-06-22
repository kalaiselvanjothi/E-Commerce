using System.ComponentModel.DataAnnotations;

namespace ShopVerse.Application.DTOs.Auth;

public class UpdateProfileDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }
}
