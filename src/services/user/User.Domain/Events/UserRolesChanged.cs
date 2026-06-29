namespace User.Domain.Events;

/// <summary>Published event: role assignment changed; Auth rebuilds JWT claims.</summary>
public record UserRolesChanged(Guid UserId, string[] Roles);
