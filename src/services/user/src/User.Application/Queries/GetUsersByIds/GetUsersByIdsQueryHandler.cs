using MediatR;
using User.Application.Models;
using User.Domain.Repositories;

namespace User.Application.Queries.GetUsersByIds;

public sealed class GetUsersByIdsQueryHandler
    : IRequestHandler<GetUsersByIdsQuery, IReadOnlyList<UserProfileDto>>
{
    private readonly IProfileRepository _profiles;
    private readonly IUserRoleRepository _roles;

    public GetUsersByIdsQueryHandler(IProfileRepository profiles, IUserRoleRepository roles)
    {
        _profiles = profiles;
        _roles = roles;
    }

    public async Task<IReadOnlyList<UserProfileDto>> Handle(
        GetUsersByIdsQuery request, CancellationToken cancellationToken)
    {
        if (request.UserIds.Count == 0)
            return Array.Empty<UserProfileDto>();

        var profiles = await _profiles.GetByIdsAsync(request.UserIds, cancellationToken);
        var roleMap = await _roles.GetRolesAsync(request.UserIds, cancellationToken);

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
