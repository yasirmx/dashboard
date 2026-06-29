namespace Authorization.Domain.Entities;

public class UserRole
{
    public Guid UserId { get; private set; }
    public string Role { get; private set; } = string.Empty;

    private UserRole() { }

    public static UserRole Create(Guid userId, string role)
        => new() { UserId = userId, Role = role };
}
