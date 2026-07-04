namespace Tasks.Api.Requests;

public record CreateTaskRequest(
    string Title,
    string? Description,
    string Priority,
    Guid? SprintId,
    Guid? AssigneeId,
    int? EstimatePoints);

public record UpdateTaskRequest(
    string Title,
    string? Description,
    string Priority,
    int? EstimatePoints);

public record ChangeStatusRequest(string Status);

public record AssignTaskRequest(Guid AssigneeId);

public record MoveToSprintRequest(Guid? SprintId);

public record ReorderTaskRequest(int OrderIndex);

public record CreateSubTaskRequest(
    string Title,
    string? Description,
    string Priority,
    Guid? AssigneeId,
    int? EstimatePoints);
