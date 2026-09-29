using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LiteFactoryApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace LiteFactoryApi.Services;

public sealed class AuthTokenService(JwtOptions options)
{
    public (string Token, DateTimeOffset ExpiresAtUtc) CreateAccessToken(LiteFactoryUser user)
    {
        var expires = DateTimeOffset.UtcNow.Add(options.AccessTokenLifetime);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("nickname", user.Nickname),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires.UtcDateTime,
            signingCredentials: new SigningCredentials(options.SigningKey, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
