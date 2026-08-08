using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShopVerse.Application.DTOs.Auth;
using ShopVerse.Application.Interfaces;
using ShopVerse.Domain.Entities;
using ShopVerse.Infrastructure.Data;

namespace ShopVerse.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly ShopVerseDbContext _context;
    private readonly IConfiguration _config;
    private readonly IEmailSender _emailSender;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IJwtService jwtService,
        ShopVerseDbContext context,
        IConfiguration config,
        IEmailSender emailSender)
    {
        _userManager   = userManager;
        _signInManager = signInManager;
        _jwtService    = jwtService;
        _context       = context;
        _config        = config;
        _emailSender   = emailSender;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            throw new InvalidOperationException("Email is already registered.");

        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber,
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        await _userManager.AddToRoleAsync(user, "Customer");

        // Create empty cart and wishlist for new user
        _context.Carts.Add(new Cart { UserId = user.Id });
        _context.Wishlists.Add(new Wishlist { UserId = user.Id });
        await _context.SaveChangesAsync();

        return await BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid email or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                throw new UnauthorizedAccessException("Account is locked. Try again later.");
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return await BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked);

        if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var user = tokenEntity.User;
        if (!user.IsActive)
            throw new UnauthorizedAccessException("User account is inactive.");

        // Rotate refresh token
        tokenEntity.IsRevoked = true;
        tokenEntity.RevokedAt = DateTime.UtcNow;

        var newRefreshToken = await CreateRefreshToken(user);
        await _context.SaveChangesAsync();

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = _jwtService.GetAccessTokenExpiry(),
            User = await MapUserProfile(user)
        };
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var tokenEntity = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken && !t.IsRevoked);

        if (tokenEntity == null) return;

        tokenEntity.IsRevoked = true;
        tokenEntity.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException("User not found.");
        return await MapUserProfile(user);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException("User not found.");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.AvatarUrl = dto.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);
        return await MapUserProfile(user);
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return; // Account enumeration protection

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = System.Net.WebUtility.UrlEncode(token);
        var encodedEmail = System.Net.WebUtility.UrlEncode(user.Email!);
        var clientUrl    = _config["ClientUrl"] ?? "http://localhost:4200";
        var resetLink    = $"{clientUrl}/auth/reset-password?email={encodedEmail}&token={encodedToken}";

        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'/></head>
<body style='font-family: Arial, sans-serif; background-color: #F8FAFC; margin: 0; padding: 20px;'>
  <div style='max-width: 600px; margin: 0 auto; background: #FFFFFF; border-radius: 16px; padding: 32px; border: 1px solid #E2E8F0; box-shadow: 0 4px 20px rgba(0,0,0,0.05);'>
    <div style='text-align: center; margin-bottom: 24px;'>
      <span style='display: inline-block; background: #4F46E5; color: #FFFFFF; font-weight: 900; font-size: 16px; padding: 8px 16px; border-radius: 8px;'>SV</span>
      <h2 style='color: #0F172A; margin-top: 12px;'>Reset Your Password</h2>
    </div>
    <p style='color: #475569; font-size: 15px;'>Hello <strong>{user.FirstName}</strong>,</p>
    <p style='color: #475569; font-size: 14px; line-height: 1.6;'>We received a request to reset the password for your ShopVerse account (<strong>{user.Email}</strong>). Click the button below to set a new password:</p>
    <div style='text-align: center; margin: 32px 0;'>
      <a href='{resetLink}' style='background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%); color: #FFFFFF; padding: 14px 28px; text-decoration: none; border-radius: 10px; font-weight: bold; font-size: 15px; display: inline-block;'>Reset Password</a>
    </div>
    <p style='color: #64748B; font-size: 12px; line-height: 1.5;'>If you did not request a password reset, you can safely ignore this email. Your password will remain unchanged.</p>
    <hr style='border: none; border-top: 1px solid #E2E8F0; margin: 24px 0;'/>
    <p style='color: #94A3B8; font-size: 11px; text-align: center;'>ShopVerse Security Team • <a href='{clientUrl}' style='color: #4F46E5;'>www.shopverse.com</a></p>
  </div>
</body>
</html>";

        await _emailSender.SendEmailAsync(user.Email!, "Reset Your ShopVerse Password", htmlBody);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new KeyNotFoundException("User not found.");

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        // Revoke all refresh tokens on password reset
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == user.Id && !t.IsRevoked)
            .ToListAsync();
        tokens.ForEach(t => { t.IsRevoked = true; t.RevokedAt = DateTime.UtcNow; });
        await _context.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new KeyNotFoundException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(GoogleLoginDto dto)
    {
        Google.Apis.Auth.GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new Google.Apis.Auth.GoogleJsonWebSignature.ValidationSettings();
            var googleClientId = _config["Authentication:Google:ClientId"];
            if (!string.IsNullOrEmpty(googleClientId))
            {
                settings.Audience = new[] { googleClientId };
            }
            payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Invalid Google ID Token: {ex.Message}");
        }

        return await ProcessOAuthUser(payload.Email, payload.GivenName ?? payload.Name, payload.FamilyName ?? "User", payload.Picture);
    }

    public async Task<AuthResponseDto> FacebookLoginAsync(FacebookLoginDto dto)
    {
        using var client = new System.Net.Http.HttpClient();
        var response = await client.GetAsync($"https://graph.facebook.com/me?fields=id,first_name,last_name,email,picture&access_token={dto.AccessToken}");

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Invalid Facebook Access Token.");

        var json = await response.Content.ReadAsStringAsync();
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;

        var email = root.TryGetProperty("email", out var e) ? e.GetString() : null;
        var firstName = root.TryGetProperty("first_name", out var fn) ? fn.GetString() : "Facebook";
        var lastName = root.TryGetProperty("last_name", out var ln) ? ln.GetString() : "User";
        var avatar = root.TryGetProperty("picture", out var p) && p.TryGetProperty("data", out var d) && d.TryGetProperty("url", out var u) ? u.GetString() : null;

        if (string.IsNullOrEmpty(email))
            email = $"{root.GetProperty("id").GetString()}@facebook.com";

        return await ProcessOAuthUser(email, firstName!, lastName!, avatar);
    }

    private async Task<AuthResponseDto> ProcessOAuthUser(string email, string firstName, string lastName, string? avatar)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new User
            {
                UserName       = email,
                Email          = email,
                FirstName      = firstName,
                LastName       = lastName,
                AvatarUrl      = avatar,
                EmailConfirmed = true,
                IsActive       = true
            };

            var randomPass = Guid.NewGuid().ToString("N") + "Aa1!";
            var createRes  = await _userManager.CreateAsync(user, randomPass);
            if (!createRes.Succeeded)
                throw new InvalidOperationException(string.Join("; ", createRes.Errors.Select(err => err.Description)));

            await _userManager.AddToRoleAsync(user, "Customer");
        }

        return await BuildAuthResponse(user);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private async Task<AuthResponseDto> BuildAuthResponse(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshToken = await CreateRefreshToken(user);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = _jwtService.GetAccessTokenExpiry(),
            User = await MapUserProfile(user)
        };
    }

    private async Task<RefreshToken> CreateRefreshToken(User user)
    {
        var expiryDays = int.Parse(_config["Jwt:RefreshTokenExpiryDays"] ?? "7");
        var token = new RefreshToken
        {
            Token = _jwtService.GenerateRefreshToken(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays)
        };
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
        return token;
    }

    private async Task<UserProfileDto> MapUserProfile(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Roles = roles.ToList()
        };
    }
}
