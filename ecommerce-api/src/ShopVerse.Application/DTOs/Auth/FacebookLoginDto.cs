using System.ComponentModel.DataAnnotations;

namespace ShopVerse.Application.DTOs.Auth;

public class FacebookLoginDto
{
    [Required(ErrorMessage = "Facebook Access Token is required")]
    public string AccessToken { get; set; } = null!;
}
