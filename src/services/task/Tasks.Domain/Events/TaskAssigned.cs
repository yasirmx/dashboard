namespace Tasks.Domain.Events;

public record TaskAssigned(Guid TaskId, Guid AssigneeId, string Title);
