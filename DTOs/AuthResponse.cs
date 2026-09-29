namespace LiteFactoryApi.DTOs;

public sealed class AuthResponse
{
    public AccountResponse User { get; set; } = new();

    public string AccessToken { get; set; } = "";

    public DateTimeOffset ExpiresAtUtc { get; set; }
}
