namespace User.Application.Models;

public record UserProfileDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? AvatarUrl,
    string? Department,
    bool IsActive,
    string[] Roles);
