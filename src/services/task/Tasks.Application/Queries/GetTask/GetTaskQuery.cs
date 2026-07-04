using MediatR;
using Tasks.Application.Models;

namespace Tasks.Application.Queries.GetTask;

public record GetTaskQuery(Guid TaskId, bool IncludeSubTasks = false) : IRequest<TaskDto?>;
