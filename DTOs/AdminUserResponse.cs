namespace LiteFactoryApi.DTOs;

public sealed class AdminUserResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; } = "";

    public string Nickname { get; set; } = "";

    public string Role { get; set; } = "";

    public DateTimeOffset CreatedAtUtc { get; set; }

    public bool IsActive { get; set; }
}
