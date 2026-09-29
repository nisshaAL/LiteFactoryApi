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

var builder = WebApplication.CreateBuilder(args);
var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
                    ?? Environment.GetEnvironmentVariable("LITEFACTORY_JWT_SIGNING_KEY");

if (string.IsNullOrWhiteSpace(jwtSigningKey) || Encoding.UTF8.GetByteCount(jwtSigningKey) < 32)
{
    throw new InvalidOperationException(
        "JWT signing key is not configured. Set Jwt:SigningKey with user secrets or LITEFACTORY_JWT_SIGNING_KEY with at least 32 bytes.");
}

var connectionString = builder.Configuration.GetConnectionString("LiteFactory")
                       ?? "Data Source=data/litefactory.db";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "LiteFactoryApi";
var audience = builder.Configuration["Jwt:Audience"] ?? "LiteFactoryClients";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<LiteFactoryDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<PasswordHasher<LiteFactoryUser>>();
builder.Services.AddScoped<AuthTokenService>();
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
            NameClaimType = JwtRegisteredClaimNames.Sub
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();
app.Logger.LogInformation("LiteFactory API starting.");

Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "data"));
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LiteFactoryDbContext>();
    db.Database.Migrate();
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
