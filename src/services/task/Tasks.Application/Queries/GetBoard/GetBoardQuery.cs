using MediatR;
using Tasks.Application.Models;

namespace Tasks.Application.Queries.GetBoard;

public record GetBoardQuery(Guid SprintId) : IRequest<BoardDto>;
