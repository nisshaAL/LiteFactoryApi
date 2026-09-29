using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LiteFactoryApi.Data;
using LiteFactoryApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiteFactoryApi.Controllers;

[ApiController]
[Authorize]
[Route("api/account")]
public sealed class AccountController(LiteFactoryDbContext db) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(subject, out var userId))
        {
            return Unauthorized(new { error = "Invalid access token." });
        }

        var user = await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(value => value.Id == userId && value.IsActive, cancellationToken);

        if (user == null)
        {
            return Unauthorized(new { error = "Account is not available." });
        }

        return Ok(new AccountResponse
        {
            Id = user.Id,
            Email = user.Email,
            Nickname = user.Nickname,
            CreatedAtUtc = user.CreatedAtUtc
        });
    }
}
