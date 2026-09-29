using LiteFactoryApi.Data;
using LiteFactoryApi.DTOs;
using LiteFactoryApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiteFactoryApi.Controllers;

[ApiController]
[Authorize(Roles = LiteFactoryRoles.Admin)]
[Route("api/admin")]
public sealed class AdminController(LiteFactoryDbContext db) : ControllerBase
{
    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<AdminUserResponse>>> ListUsers(CancellationToken cancellationToken)
    {
        var users = await db.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Ok(users
            .OrderBy(user => user.CreatedAtUtc)
            .ThenBy(user => user.Nickname)
            .Select(ToAdminUserResponse)
            .ToList());
    }

    [HttpGet("users/{id}")]
    public async Task<ActionResult<AdminUserResponse>> GetUser(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var userId))
        {
            return BadRequest(new { error = "Invalid user ID." });
        }

        var user = await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(value => value.Id == userId, cancellationToken);
        if (user == null)
        {
            return NotFound(new { error = "User was not found." });
        }

        return Ok(ToAdminUserResponse(user));
    }

    [HttpPut("users/{id}/role")]
    public async Task<ActionResult<AdminUserResponse>> ChangeUserRole(
        string id,
        ChangeUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var userId))
        {
            return BadRequest(new { error = "Invalid user ID." });
        }

        var requestedRole = NormalizeRole(request.Role);
        if (requestedRole == null)
        {
            return BadRequest(new { error = "Role must be USER, DEVELOPER, or ADMIN." });
        }

        var user = await db.Users.SingleOrDefaultAsync(value => value.Id == userId, cancellationToken);
        if (user == null)
        {
            return NotFound(new { error = "User was not found." });
        }

        if (user.Role == LiteFactoryRoles.Admin && requestedRole != LiteFactoryRoles.Admin)
        {
            var adminCount = await db.Users.CountAsync(
                value => value.Role == LiteFactoryRoles.Admin,
                cancellationToken);
            if (adminCount <= 1)
            {
                return Conflict(new { error = "The last administrator cannot be demoted." });
            }
        }

        user.Role = requestedRole;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(ToAdminUserResponse(user));
    }

    private static string? NormalizeRole(string role)
    {
        var normalized = role.Trim().ToUpperInvariant();
        return normalized switch
        {
            LiteFactoryRoles.User => LiteFactoryRoles.User,
            LiteFactoryRoles.Developer => LiteFactoryRoles.Developer,
            LiteFactoryRoles.Admin => LiteFactoryRoles.Admin,
            _ => null
        };
    }

    private static AdminUserResponse ToAdminUserResponse(LiteFactoryUser user)
    {
        return new AdminUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Nickname = user.Nickname,
            Role = user.Role,
            CreatedAtUtc = user.CreatedAtUtc,
            IsActive = user.IsActive
        };
    }
}
