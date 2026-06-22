using System.ComponentModel.DataAnnotations;

namespace ShopVerse.Application.DTOs.Auth;

public class RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
