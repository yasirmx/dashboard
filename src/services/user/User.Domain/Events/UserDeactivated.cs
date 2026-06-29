namespace User.Domain.Events;

/// <summary>Published event: user disabled; consumers may unassign their tasks.</summary>
public record UserDeactivated(Guid UserId);
