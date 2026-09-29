namespace LiteFactoryApi.Models;

public sealed class LiteFactoryUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = "";

    public string NormalizedEmail { get; set; } = "";

    public string Nickname { get; set; } = "";

    public string NormalizedNickname { get; set; } = "";

    public string PasswordHash { get; set; } = "";

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public bool IsActive { get; set; } = true;

    public string Role { get; set; } = LiteFactoryRoles.User;
}
