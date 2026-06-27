using Authorization.Application.Abstractions;
using Authorization.Domain.Entities;
using Authorization.Domain.Repositories;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace Authorization.Application.Commands.Refresh;

public sealed class RefreshCommandHandler : IRequestHandler<RefreshCommand, RefreshResult>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ICredentialRepository _credentials;
    private readonly IUserRoleRepository _userRoles;
    private readonly ITokenService _tokenService;

    public RefreshCommandHandler(
        IRefreshTokenRepository refreshTokens,
        ICredentialRepository credentials,
        IUserRoleRepository userRoles,
        ITokenService tokenService)
    {
        _refreshTokens = refreshTokens;
        _credentials = credentials;
        _userRoles = userRoles;
        _tokenService = tokenService;
    }

    public async Task<RefreshResult> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var stored = await _refreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new UnauthorizedAccessException("Refresh token not found.");

        if (!stored.IsActive)
            throw new UnauthorizedAccessException("Refresh token is expired or revoked.");

        var credential = await _credentials.GetByIdAsync(stored.UserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        stored.Revoke();

        var roles = await _userRoles.GetRolesAsync(credential.Id, cancellationToken);
        var accessToken = _tokenService.GenerateAccessToken(credential.Id, credential.Email, roles);

        var (rawToken, newHash, expiresUtc) = _tokenService.GenerateRefreshToken();
        var newRefreshToken = RefreshToken.Create(credential.Id, newHash, expiresUtc);

        await _refreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await _refreshTokens.SaveChangesAsync(cancellationToken);

        return new RefreshResult(accessToken, rawToken);
    }

    private static string HashToken(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToBase64String(bytes);
    }
}
