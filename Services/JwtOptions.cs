using Microsoft.IdentityModel.Tokens;

namespace LiteFactoryApi.Services;

public sealed record JwtOptions(
    string Issuer,
    string Audience,
    SecurityKey SigningKey,
    TimeSpan AccessTokenLifetime);
