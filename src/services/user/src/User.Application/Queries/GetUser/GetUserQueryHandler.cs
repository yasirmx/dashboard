using MediatR;
using User.Application.Models;
using User.Domain.Repositories;

namespace User.Application.Queries.GetUser;

public sealed class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserProfileDto?>
{
    private readonly IProfileRepository _profiles;
    private readonly IUserRoleRepository _roles;

    public GetUserQueryHandler(IProfileRepository profiles, IUserRoleRepository roles)
    {
        _profiles = profiles;
        _roles = roles;
    }

    public async Task<UserProfileDto?> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var profile = await _profiles.GetByIdAsync(request.UserId, cancellationToken);
        if (profile is null) return null;

        var roles = await _roles.GetRolesAsync(request.UserId, cancellationToken);

        return new UserProfileDto(
            profile.UserId,
            profile.Email,
            profile.FirstName,
            profile.LastName,
            profile.AvatarUrl,
            profile.Department,
            profile.IsActive,
            roles.ToArray());
    }
}
