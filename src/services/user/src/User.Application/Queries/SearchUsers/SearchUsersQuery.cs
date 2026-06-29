using MediatR;
using User.Application.Models;

namespace User.Application.Queries.SearchUsers;

public record SearchUsersQuery(string? Search, string? Role) : IRequest<IReadOnlyList<UserProfileDto>>;
