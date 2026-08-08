using System.ComponentModel.DataAnnotations;

namespace ShopVerse.Application.DTOs.Auth;

public class GoogleLoginDto
{
    [Required(ErrorMessage = "Google ID Token is required")]
    public string IdToken { get; set; } = null!;
}
