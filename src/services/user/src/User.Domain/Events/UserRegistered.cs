namespace User.Domain.Events;

/// <summary>Consumed event: a credential was created in Auth; create the matching profile.</summary>
public record UserRegistered(Guid UserId, string Email, string FirstName, string LastName);
