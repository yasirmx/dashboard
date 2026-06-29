namespace Authorization.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresUtc { get; private set; }
    public DateTime? RevokedUtc { get; private set; }

    public bool IsActive => RevokedUtc is null && DateTime.UtcNow < ExpiresUtc;

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTime expiresUtc)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresUtc = expiresUtc
        };
    }

    public void Revoke()
    {
        RevokedUtc = DateTime.UtcNow;
    }
}
