using Tasks.Application.Models;
using Tasks.Domain.Entities;

namespace Tasks.Application.Mappings;

internal static class TaskMappings
{
    public static TaskDto ToDto(this TaskItem item)
        => new(
            item.Id,
            item.ParentTaskId,
            item.Title,
            item.Description,
            item.Status.ToString(),
            item.Priority.ToString(),
            item.SprintId,
            item.AssigneeId,
            item.CreatedByUserId,
            item.EstimatePoints,
            item.OrderIndex,
            item.CreatedUtc,
            item.UpdatedUtc,
            item.ClosedUtc,
            item.Tags.Select(t => t.Tag).ToList());
}
