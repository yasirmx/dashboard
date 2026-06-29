using MediatR;
using User.Application.Models;
using User.Domain.Repositories;

namespace User.Application.Queries.SearchUsers;

public sealed class SearchUsersQueryHandler
    : IRequestHandler<SearchUsersQuery, IReadOnlyList<UserProfileDto>>
{
    private readonly IProfileRepository _profiles;
    private readonly IUserRoleRepository _roles;

    public SearchUsersQueryHandler(IProfileRepository profiles, IUserRoleRepository roles)
    {
        _profiles = profiles;
        _roles = roles;
    }

    public async Task<IReadOnlyList<UserProfileDto>> Handle(
        SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var profiles = await _profiles.SearchAsync(request.Search, request.Role, cancellationToken);
        if (profiles.Count == 0)
            return Array.Empty<UserProfileDto>();

        var roleMap = await _roles.GetRolesAsync(profiles.Select(p => p.UserId), cancellationToken);

        return profiles
            .Select(p => new UserProfileDto(
                p.UserId,
                p.Email,
                p.FirstName,
                p.LastName,
                p.AvatarUrl,
                p.Department,
                p.IsActive,
                roleMap.TryGetValue(p.UserId, out var r) ? r : Array.Empty<string>()))
            .ToList();
    }
}
