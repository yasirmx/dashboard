namespace Authorization.Domain.Entities;

public class Credential
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool EmailConfirmed { get; private set; }
    public DateTime CreatedUtc { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresUtc { get; private set; }

    private Credential() { }

    public static Credential Create(string email, string passwordHash)
    {
        return new Credential
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            EmailConfirmed = false,
            CreatedUtc = DateTime.UtcNow
        };
    }

    public void SetPasswordResetToken(string token, DateTime expiresUtc)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiresUtc = expiresUtc;
    }

    public void ResetPassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiresUtc = null;
    }

    public bool IsPasswordResetTokenValid(string token)
        => PasswordResetToken == token
           && PasswordResetTokenExpiresUtc.HasValue
           && PasswordResetTokenExpiresUtc.Value > DateTime.UtcNow;
}
