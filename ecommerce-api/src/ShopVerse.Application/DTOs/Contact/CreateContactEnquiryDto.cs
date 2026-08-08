using System.ComponentModel.DataAnnotations;

namespace ShopVerse.Application.DTOs.Contact;

public class CreateContactEnquiryDto
{
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full Name must be between 2 and 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    [Required(ErrorMessage = "Subject is required.")]
    public string Subject { get; set; } = string.Empty;

    public string? OrderNumber { get; set; }

    [Required(ErrorMessage = "Message is required.")]
    [MinLength(10, ErrorMessage = "Message must be at least 10 characters long.")]
    public string Message { get; set; } = string.Empty;
}
