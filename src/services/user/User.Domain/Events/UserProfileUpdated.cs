namespace User.Domain.Events;

/// <summary>Published event: profile changed; consumers refresh denormalized name/avatar.</summary>
public record UserProfileUpdated(Guid UserId, string FirstName, string LastName, string? AvatarUrl);
