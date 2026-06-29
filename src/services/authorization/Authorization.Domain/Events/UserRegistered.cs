namespace Authorization.Domain.Events;

public record UserRegistered(Guid UserId, string Email, string FirstName, string LastName);
