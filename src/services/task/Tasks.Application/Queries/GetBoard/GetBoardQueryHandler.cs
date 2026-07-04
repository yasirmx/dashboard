using MediatR;
using Tasks.Application.Mappings;
using Tasks.Application.Models;
using Tasks.Domain.Enums;
using Tasks.Domain.Repositories;

namespace Tasks.Application.Queries.GetBoard;

public sealed class GetBoardQueryHandler : IRequestHandler<GetBoardQuery, BoardDto>
{
    private readonly ITaskRepository _tasks;

    public GetBoardQueryHandler(ITaskRepository tasks) => _tasks = tasks;

    public async System.Threading.Tasks.Task<BoardDto> Handle(GetBoardQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _tasks.GetBoardAsync(request.SprintId, cancellationToken);
        var byStatus = tasks.GroupBy(t => t.Status).ToDictionary(g => g.Key, g => g.ToList());

        var columns = Enum.GetValues<TaskItemStatus>()
            .Select(status => new BoardColumnDto(
                status.ToString(),
                byStatus.TryGetValue(status, out var items)
                    ? items.OrderBy(t => t.OrderIndex).Select(t => t.ToDto()).ToList()
                    : []))
            .ToList();

        return new BoardDto(request.SprintId, columns);
    }
}
