using MediatR;

namespace Tasks.Application.Commands.ReorderTask;

public record ReorderTaskCommand(Guid TaskId, int OrderIndex) : IRequest;
