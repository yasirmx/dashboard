using MediatR;
using User.Application.Models;

namespace User.Application.Queries.GetUsersByIds;

public record GetUsersByIdsQuery(IReadOnlyList<Guid> UserIds) : IRequest<IReadOnlyList<UserProfileDto>>;
