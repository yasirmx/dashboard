using Authorization.Application.Abstractions;
using Authorization.Domain.Entities;
using Authorization.Domain.Repositories;
using MediatR;

namespace Authorization.Application.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly ICredentialRepository _credentials;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUserRoleRepository _userRoles;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        ICredentialRepository credentials,
        IRefreshTokenRepository refreshTokens,
        IUserRoleRepository userRoles,
        IPasswordHasher hasher,
        ITokenService tokenService)
    {
        _credentials = credentials;
        _refreshTokens = refreshTokens;
        _userRoles = userRoles;
        _hasher = hasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var credential = await _credentials.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!_hasher.Verify(request.Password, credential.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var roles = await _userRoles.GetRolesAsync(credential.Id, cancellationToken);
        var accessToken = _tokenService.GenerateAccessToken(credential.Id, credential.Email, roles);

        var (rawToken, tokenHash, expiresUtc) = _tokenService.GenerateRefreshToken();
        var refreshToken = RefreshToken.Create(credential.Id, tokenHash, expiresUtc);

        await _refreshTokens.AddAsync(refreshToken, cancellationToken);
        await _refreshTokens.SaveChangesAsync(cancellationToken);

        return new LoginResult(accessToken, rawToken);
    }
}
