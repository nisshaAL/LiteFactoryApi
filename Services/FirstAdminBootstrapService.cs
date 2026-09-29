using LiteFactoryApi.Data;
using LiteFactoryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LiteFactoryApi.Services;

public sealed class FirstAdminBootstrapService(
    LiteFactoryDbContext db,
    IConfiguration configuration,
    ILogger<FirstAdminBootstrapService> logger)
{
    private const string AdminUserIdConfigurationKey = "LiteFactoryBootstrap:AdminUserId";

    public async Task BootstrapAsync(CancellationToken cancellationToken = default)
    {
        var configuredAdminUserId = configuration[AdminUserIdConfigurationKey];
        if (string.IsNullOrWhiteSpace(configuredAdminUserId))
        {
            return;
        }

        if (!Guid.TryParse(configuredAdminUserId, out var adminUserId))
        {
            logger.LogWarning("First admin bootstrap skipped. Configured AdminUserId is not a valid user ID.");
            return;
        }

        var targetUser = await db.Users.SingleOrDefaultAsync(
            user => user.Id == adminUserId,
            cancellationToken);
        if (targetUser == null)
        {
            logger.LogWarning("First admin bootstrap skipped. UserId={UserId} was not found.", adminUserId);
            return;
        }

        if (targetUser.Role == LiteFactoryRoles.Admin)
        {
            logger.LogInformation("First admin bootstrap skipped. UserId={UserId} is already ADMIN.", adminUserId);
            return;
        }

        var adminExists = await db.Users.AnyAsync(
            user => user.Role == LiteFactoryRoles.Admin,
            cancellationToken);
        if (adminExists)
        {
            logger.LogInformation("First admin bootstrap skipped. An ADMIN account already exists.");
            return;
        }

        targetUser.Role = LiteFactoryRoles.Admin;
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("First admin bootstrap promoted UserId={UserId} to ADMIN.", adminUserId);
    }
}
