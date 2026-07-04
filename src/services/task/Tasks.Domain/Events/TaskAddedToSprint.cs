namespace Tasks.Domain.Events;

public record TaskAddedToSprint(Guid TaskId, Guid SprintId);
