using Authorization.Application.Abstractions;
using Authorization.Domain.Repositories;
using MediatR;

namespace Authorization.Application.Queries.Me;

public sealed class MeQueryHandler : IRequestHandler<MeQuery, MeResult>
{
    private readonly ITokenService _tokenService;
    private readonly ICredentialRepository _credentials;
    private readonly IUserRoleRepository _userRoles;

    public MeQueryHandler(
        ITokenService tokenService,
        ICredentialRepository credentials,
        IUserRoleRepository userRoles)
    {
        _tokenService = tokenService;
        _credentials = credentials;
        _userRoles = userRoles;
    }

    public async Task<MeResult> Handle(MeQuery request, CancellationToken cancellationToken)
    {
        var userId = _tokenService.GetUserIdFromToken(request.AccessToken)
            ?? throw new UnauthorizedAccessException("Invalid access token.");

        var credential = await _credentials.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        var roles = await _userRoles.GetRolesAsync(userId, cancellationToken);

        return new MeResult(userId, credential.Email, [.. roles]);
    }
}
