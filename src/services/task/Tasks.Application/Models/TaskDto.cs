namespace Tasks.Application.Models;

public record TaskDto(
    Guid Id,
    Guid? ParentTaskId,
    string Title,
    string? Description,
    string Status,
    string Priority,
    Guid? SprintId,
    Guid? AssigneeId,
    Guid CreatedByUserId,
    int? EstimatePoints,
    int OrderIndex,
    DateTime CreatedUtc,
    DateTime UpdatedUtc,
    DateTime? ClosedUtc,
    IReadOnlyList<string> Tags);
