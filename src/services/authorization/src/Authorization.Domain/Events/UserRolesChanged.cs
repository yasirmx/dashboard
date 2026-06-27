namespace Authorization.Domain.Events;

/// <summary>Consumed event: updates the local UserRoles copy to rebuild JWT claims.</summary>
public record UserRolesChanged(Guid UserId, string[] Roles);
