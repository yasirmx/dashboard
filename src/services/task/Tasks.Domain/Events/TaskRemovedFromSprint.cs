namespace Tasks.Domain.Events;

public record TaskRemovedFromSprint(Guid TaskId, Guid SprintId);
