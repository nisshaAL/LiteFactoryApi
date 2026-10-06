using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using LiteFactoryApi.Data;
using LiteFactoryApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LiteFactoryApi.Services;

public sealed class FirstAdminBootstrapService(
    LiteFactoryDbContext db,
    IConfiguration configuration,
    PasswordHasher<LiteFactoryUser> passwordHasher,
    ILogger<FirstAdminBootstrapService> logger)
{
    private static readonly Regex NicknameRegex = new("^[A-Za-z0-9_]{3,20}$", RegexOptions.Compiled);

    private const string BootstrapEmailKey = "LITEFACTORY_BOOTSTRAP_ADMIN_EMAIL";
    private const string BootstrapNicknameKey = "LITEFACTORY_BOOTSTRAP_ADMIN_NICKNAME";
    private const string BootstrapPasswordKey = "LITEFACTORY_BOOTSTRAP_ADMIN_PASSWORD";

    public async Task BootstrapAsync(CancellationToken cancellationToken = default)
    {
        var email = configuration[BootstrapEmailKey]?.Trim();
        var nickname = configuration[BootstrapNicknameKey]?.Trim();
        var password = configuration[BootstrapPasswordKey];

        var hasAnyBootstrapValue = !string.IsNullOrWhiteSpace(email) ||
                                   !string.IsNullOrWhiteSpace(nickname) ||
                                   !string.IsNullOrWhiteSpace(password);
        if (!hasAnyBootstrapValue)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(nickname) ||
            string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("First admin bootstrap skipped. All bootstrap environment variables must be configured.");
            return;
        }

        var validationError = ValidateBootstrapValues(email, nickname, password);
        if (validationError != null)
        {
            logger.LogWarning("First admin bootstrap skipped. {ValidationError}", validationError);
            return;
        }

        var normalizedEmail = Normalize(email);
        var normalizedNickname = Normalize(nickname);

        var existingAdmin = await db.Users.AnyAsync(
            user => user.Role == LiteFactoryRoles.Admin,
            cancellationToken);
        if (existingAdmin)
        {
            logger.LogInformation("First admin bootstrap skipped. An ADMIN account already exists.");
            return;
        }

        var existingUser = await db.Users.SingleOrDefaultAsync(
            user => user.NormalizedEmail == normalizedEmail || user.NormalizedNickname == normalizedNickname,
            cancellationToken);

        if (existingUser != null)
        {
            if (existingUser.NormalizedEmail != normalizedEmail ||
                existingUser.NormalizedNickname != normalizedNickname)
            {
                logger.LogWarning("First admin bootstrap skipped. Email or nickname belongs to a different existing account.");
                return;
            }

            if (!existingUser.IsActive)
            {
                logger.LogWarning("First admin bootstrap skipped. Target account is inactive. UserId={UserId}", existingUser.Id);
                return;
            }

            existingUser.Role = LiteFactoryRoles.Admin;
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("First admin bootstrap promoted existing account to ADMIN. UserId={UserId}, Nickname={Nickname}", existingUser.Id, existingUser.Nickname);
            return;
        }

        var user = new LiteFactoryUser
        {
            Email = email,
            NormalizedEmail = normalizedEmail,
            Nickname = nickname,
            NormalizedNickname = normalizedNickname,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsActive = true,
            Role = LiteFactoryRoles.Admin
        };
        user.PasswordHash = passwordHasher.HashPassword(user, password);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("First admin bootstrap created ADMIN account. UserId={UserId}, Nickname={Nickname}", user.Id, user.Nickname);
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static string? ValidateBootstrapValues(string email, string nickname, string password)
    {
        if (email.Length is < 3 or > 254 || !new EmailAddressAttribute().IsValid(email))
        {
            return "Bootstrap email must be a valid address up to 254 characters.";
        }

        if (!NicknameRegex.IsMatch(nickname))
        {
            return "Bootstrap nickname must be 3-20 characters and may contain only letters, numbers, and underscore.";
        }

        if (password.Length < 8)
        {
            return "Bootstrap password does not meet the minimum length requirement.";
        }

        return null;
    }
}
