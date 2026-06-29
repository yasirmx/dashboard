using MediatR;
using User.Application.Models;

namespace User.Application.Queries.GetUser;

public record GetUserQuery(Guid UserId) : IRequest<UserProfileDto?>;
