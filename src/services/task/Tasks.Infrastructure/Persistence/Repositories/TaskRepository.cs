using Tasks.Domain.Entities;
using Tasks.Domain.Enums;
using Tasks.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Tasks.Infrastructure.Persistence.Repositories;

public sealed class TaskRepository : ITaskRepository
{
    private readonly TaskContext _context;

    public TaskRepository(TaskContext context) => _context = context;

    public System.Threading.Tasks.Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct)
        => _context.Tasks
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public System.Threading.Tasks.Task<TaskItem?> GetByIdWithSubTasksAsync(Guid id, CancellationToken ct)
        => _context.Tasks
            .Include(t => t.Tags)
            .Include(t => t.SubTasks)
                .ThenInclude(s => s.Tags)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async System.Threading.Tasks.Task<IReadOnlyList<TaskItem>> GetAsync(
        Guid? sprintId,
        Guid? assigneeId,
        TaskItemStatus? status,
        Guid? parentId,
        CancellationToken ct)
    {
        var query = _context.Tasks.Include(t => t.Tags).AsQueryable();

        if (sprintId.HasValue)      query = query.Where(t => t.SprintId == sprintId);
        if (assigneeId.HasValue)    query = query.Where(t => t.AssigneeId == assigneeId);
        if (status.HasValue)        query = query.Where(t => t.Status == status);
        if (parentId.HasValue)      query = query.Where(t => t.ParentTaskId == parentId);

        return await query.OrderBy(t => t.OrderIndex).ThenBy(t => t.CreatedUtc).ToListAsync(ct);
    }

    public async System.Threading.Tasks.Task<IReadOnlyList<TaskItem>> GetSubTasksAsync(Guid parentTaskId, CancellationToken ct)
        => await _context.Tasks
            .Include(t => t.Tags)
            .Where(t => t.ParentTaskId == parentTaskId)
            .OrderBy(t => t.OrderIndex)
            .ToListAsync(ct);

    public System.Threading.Tasks.Task<bool> HasOpenSubTasksAsync(Guid parentTaskId, CancellationToken ct)
        => _context.Tasks.AnyAsync(
            t => t.ParentTaskId == parentTaskId && t.Status != TaskItemStatus.Closed, ct);

    public async System.Threading.Tasks.Task<IReadOnlyList<TaskItem>> GetBySprintIdAsync(Guid sprintId, CancellationToken ct)
        => await _context.Tasks
            .Where(t => t.SprintId == sprintId)
            .ToListAsync(ct);

    public async System.Threading.Tasks.Task<IReadOnlyList<TaskItem>> GetBoardAsync(Guid sprintId, CancellationToken ct)
        => await _context.Tasks
            .Include(t => t.Tags)
            .Where(t => t.SprintId == sprintId && t.ParentTaskId == null)
            .OrderBy(t => t.Status)
            .ThenBy(t => t.OrderIndex)
            .ToListAsync(ct);

    public async System.Threading.Tasks.Task AddAsync(TaskItem task, CancellationToken ct)
        => await _context.Tasks.AddAsync(task, ct);

    public void Remove(TaskItem task)
        => _context.Tasks.Remove(task);

    public System.Threading.Tasks.Task SaveChangesAsync(CancellationToken ct)
        => _context.SaveChangesAsync(ct);
}
