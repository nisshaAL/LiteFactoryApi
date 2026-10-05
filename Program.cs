using System.IdentityModel.Tokens.Jwt;
using System.Text;
using LiteFactoryApi.Data;
using LiteFactoryApi.Models;
using LiteFactoryApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);
var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(renderPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");
}

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
                    ?? Environment.GetEnvironmentVariable("LITEFACTORY_JWT_SIGNING_KEY");

if (string.IsNullOrWhiteSpace(jwtSigningKey) || Encoding.UTF8.GetByteCount(jwtSigningKey) < 32)
{
    throw new InvalidOperationException(
        "JWT signing key is not configured. Set Jwt:SigningKey with user secrets or LITEFACTORY_JWT_SIGNING_KEY with at least 32 bytes.");
}

var databaseSettings = DatabaseConfiguration.Resolve(builder.Configuration);
var issuer = builder.Configuration["Jwt:Issuer"] ?? "LiteFactoryApi";
var audience = builder.Configuration["Jwt:Audience"] ?? "LiteFactoryClients";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
if (databaseSettings.Provider == LiteFactoryDatabaseProvider.PostgreSql)
{
    builder.Services.AddDbContext<LiteFactoryDbContext, LiteFactoryPostgresDbContext>(
        options => options.UseNpgsql(databaseSettings.ConnectionString));
}
else
{
    builder.Services.AddDbContext<LiteFactoryDbContext, LiteFactorySqliteDbContext>(
        options => options.UseSqlite(databaseSettings.ConnectionString));
}

builder.Services.AddScoped<PasswordHasher<LiteFactoryUser>>();
builder.Services.AddScoped<AuthTokenService>();
builder.Services.AddScoped<FirstAdminBootstrapService>();
builder.Services.AddSingleton(new JwtOptions(issuer, audience, signingKey, TimeSpan.FromMinutes(30)));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();
app.Logger.LogInformation("LiteFactory API starting. Database provider: {DatabaseProvider}.", databaseSettings.Provider);

if (databaseSettings.Provider == LiteFactoryDatabaseProvider.Sqlite)
{
    Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "data"));
}
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LiteFactoryDbContext>();
    db.Database.Migrate();

    var firstAdminBootstrap = scope.ServiceProvider.GetRequiredService<FirstAdminBootstrapService>();
    await firstAdminBootstrap.BootstrapAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
