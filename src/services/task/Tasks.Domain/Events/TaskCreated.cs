namespace Tasks.Domain.Events;

public record TaskCreated(Guid TaskId, Guid? SprintId, Guid? AssigneeId, string Title);
