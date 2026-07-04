namespace Tasks.Domain.Events;

public record TaskStatusChanged(Guid TaskId, Guid? SprintId, string OldStatus, string NewStatus);
