namespace LiteFactoryApi.DTOs;

public sealed class AccountResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; } = "";

    public string Nickname { get; set; } = "";

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string Role { get; set; } = "";
}
