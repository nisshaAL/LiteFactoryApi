using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using LiteFactoryApi.Data;
using LiteFactoryApi.DTOs;
using LiteFactoryApi.Models;
using LiteFactoryApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiteFactoryApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    LiteFactoryDbContext db,
    PasswordHasher<LiteFactoryUser> passwordHasher,
    AuthTokenService tokenService,
    ILogger<AuthController> logger) : ControllerBase
{
    private static readonly Regex NicknameRegex = new("^[A-Za-z0-9_]{3,20}$", RegexOptions.Compiled);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var validationError = ValidateRegistration(request);
        if (validationError != null)
        {
            return BadRequest(new { error = validationError });
        }

        var email = request.Email.Trim();
        var nickname = request.Nickname.Trim();
        var normalizedEmail = Normalize(email);
        var normalizedNickname = Normalize(nickname);

        if (await db.Users.AnyAsync(user => user.NormalizedEmail == normalizedEmail, cancellationToken))
        {
            return Conflict(new { error = "Email is already registered." });
        }

        if (await db.Users.AnyAsync(user => user.NormalizedNickname == normalizedNickname, cancellationToken))
        {
            return Conflict(new { error = "Nickname is already registered." });
        }

        var user = new LiteFactoryUser
        {
            Email = email,
            NormalizedEmail = normalizedEmail,
            Nickname = nickname,
            NormalizedNickname = normalizedNickname,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsActive = true
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Conflict(new { error = "Email or nickname is already registered." });
        }

        logger.LogInformation("LiteFactory account registered. UserId={UserId}, Nickname={Nickname}", user.Id, user.Nickname);
        return CreatedAtAction(nameof(Register), ToAccountResponse(user));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = Normalize(request.Email);
        var user = await db.Users.SingleOrDefaultAsync(value => value.NormalizedEmail == normalizedEmail, cancellationToken);
        if (user == null || !user.IsActive)
        {
            logger.LogInformation("LiteFactory login failed.");
            return Unauthorized(new { error = "Invalid email or password." });
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            logger.LogInformation("LiteFactory login failed. UserId={UserId}", user.Id);
            return Unauthorized(new { error = "Invalid email or password." });
        }

        var (token, expiresAtUtc) = tokenService.CreateAccessToken(user);
        logger.LogInformation("LiteFactory login succeeded. UserId={UserId}", user.Id);

        return Ok(new AuthResponse
        {
            User = ToAccountResponse(user),
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc
        });
    }

    private static string? ValidateRegistration(RegisterRequest request)
    {
        var email = request.Email.Trim();
        var nickname = request.Nickname.Trim();

        if (email.Length is < 3 or > 254 || !new EmailAddressAttribute().IsValid(email))
        {
            return "Email must be a valid address up to 254 characters.";
        }

        if (!NicknameRegex.IsMatch(nickname))
        {
            return "Nickname must be 3-20 characters and may contain only letters, numbers, and underscore.";
        }

        if (request.Password.Length < 8)
        {
            return "Password must be at least 8 characters.";
        }

        return null;
    }

    private static AccountResponse ToAccountResponse(LiteFactoryUser user)
    {
        return new AccountResponse
        {
            Id = user.Id,
            Email = user.Email,
            Nickname = user.Nickname,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }
}
