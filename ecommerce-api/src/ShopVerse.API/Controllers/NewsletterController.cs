using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopVerse.Application.Common;
using ShopVerse.Application.DTOs.Newsletter;
using ShopVerse.Domain.Entities;
using ShopVerse.Infrastructure.Data;

namespace ShopVerse.API.Controllers;

[ApiController]
[Route("api/newsletter")]
[AllowAnonymous]
[Produces("application/json")]
public class NewsletterController : ControllerBase
{
    private readonly ShopVerseDbContext _context;

    public NewsletterController(ShopVerseDbContext context) => _context = context;

    /// <summary>
    /// Subscribe an email address to the newsletter with duplicate prevention.
    /// </summary>
    [HttpPost("subscribe")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Subscribe([FromBody] SubscribeNewsletterDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<object>.Fail("Validation failed", errors));
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        var existing = await _context.NewsletterSubscribers
            .FirstOrDefaultAsync(s => s.Email == normalizedEmail);

        if (existing != null)
        {
            if (existing.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail("This email address is already subscribed to the ShopVerse newsletter."));
            }

            // Re-activate existing subscriber
            existing.IsActive = true;
            existing.SubscribedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(new { email = normalizedEmail }, "Welcome back! Your newsletter subscription has been re-activated."));
        }

        var subscriber = new NewsletterSubscriber
        {
            Email        = normalizedEmail,
            SubscribedAt = DateTime.UtcNow,
            IsActive     = true
        };

        _context.NewsletterSubscribers.Add(subscriber);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<object>.Ok(new { id = subscriber.Id, email = normalizedEmail }, "Thank you for subscribing to the ShopVerse newsletter!"));
    }
}
