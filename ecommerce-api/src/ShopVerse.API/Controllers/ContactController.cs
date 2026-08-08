using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.Contact;
using ShopVerse.Domain.Entities;
using ShopVerse.Infrastructure.Data;

namespace ShopVerse.API.Controllers;

[ApiController]
[Route("api/contact")]
[AllowAnonymous]
[Produces("application/json")]
public class ContactController : ControllerBase
{
    private readonly ShopVerseDbContext _context;

    public ContactController(ShopVerseDbContext context) => _context = context;

    /// <summary>
    /// Submit a customer support enquiry form into PostgreSQL database.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> SubmitEnquiry([FromBody] CreateContactEnquiryDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<object>.Fail("Validation failed", errors));
        }

        var enquiry = new ContactEnquiry
        {
            FullName    = dto.FullName.Trim(),
            Email       = dto.Email.Trim().ToLowerInvariant(),
            Phone       = dto.Phone?.Trim(),
            Subject     = dto.Subject.Trim(),
            OrderNumber = dto.OrderNumber?.Trim(),
            Message     = dto.Message.Trim(),
            CreatedAt   = DateTime.UtcNow,
            IsResolved  = false
        };

        _context.ContactEnquiries.Add(enquiry);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { id = enquiry.Id }, "Thank you! Your message has been submitted successfully. Our support team will get back to you within 24 hours."));
    }
}
