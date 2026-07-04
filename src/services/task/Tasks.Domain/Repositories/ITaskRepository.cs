using Tasks.Domain.Entities;
using Tasks.Domain.Enums;

namespace Tasks.Domain.Repositories;

public interface ITaskRepository
{
    System.Threading.Tasks.Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    System.Threading.Tasks.Task<TaskItem?> GetByIdWithSubTasksAsync(Guid id, CancellationToken ct = default);
    System.Threading.Tasks.Task<IReadOnlyList<TaskItem>> GetAsync(
        Guid? sprintId,
        Guid? assigneeId,
        TaskItemStatus? status,
        Guid? parentId,
        CancellationToken ct = default);
    System.Threading.Tasks.Task<IReadOnlyList<TaskItem>> GetSubTasksAsync(Guid parentTaskId, CancellationToken ct = default);
    System.Threading.Tasks.Task<bool> HasOpenSubTasksAsync(Guid parentTaskId, CancellationToken ct = default);
    System.Threading.Tasks.Task<IReadOnlyList<TaskItem>> GetBySprintIdAsync(Guid sprintId, CancellationToken ct = default);
    System.Threading.Tasks.Task<IReadOnlyList<TaskItem>> GetBoardAsync(Guid sprintId, CancellationToken ct = default);
    System.Threading.Tasks.Task AddAsync(TaskItem task, CancellationToken ct = default);
    void Remove(TaskItem task);
    System.Threading.Tasks.Task SaveChangesAsync(CancellationToken ct = default);
}
