namespace User.Domain.Entities;

public class Profile
{
    public Guid UserId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? AvatarUrl { get; private set; }
    public string? Department { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedUtc { get; private set; }

    private Profile() { }

    public static Profile Create(
        Guid userId,
        string email,
        string firstName,
        string lastName,
        string? avatarUrl = null,
        string? department = null)
    {
        return new Profile
        {
            UserId = userId,
            Email = email.ToLowerInvariant(),
            FirstName = firstName,
            LastName = lastName,
            AvatarUrl = avatarUrl,
            Department = department,
            IsActive = true,
            CreatedUtc = DateTime.UtcNow
        };
    }

    public void UpdateProfile(string firstName, string lastName, string? avatarUrl, string? department)
    {
        FirstName = firstName;
        LastName = lastName;
        AvatarUrl = avatarUrl;
        Department = department;
    }

    public void Deactivate() => IsActive = false;
}
