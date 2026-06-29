using Authorization.Domain.Repositories;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace Authorization.Application.Commands.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenRepository _refreshTokens;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokens)
        => _refreshTokens = refreshTokens;

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var stored = await _refreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (stored is null || !stored.IsActive)
            return; // idempotent

        stored.Revoke();
        await _refreshTokens.SaveChangesAsync(cancellationToken);
    }

    private static string HashToken(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToBase64String(bytes);
    }
}
