using System.ComponentModel.DataAnnotations;

namespace ShopVerse.Application.DTOs.Newsletter;

public class SubscribeNewsletterDto
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;
}
