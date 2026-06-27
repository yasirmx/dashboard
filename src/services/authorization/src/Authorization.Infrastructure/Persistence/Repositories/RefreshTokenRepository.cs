using Authorization.Domain.Entities;
using Authorization.Domain.Repositories;
using Authorization.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthorizationContext _context;

    public RefreshTokenRepository(AuthorizationContext context) => _context = context;

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct)
        => _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct)
        => await _context.RefreshTokens.AddAsync(token, ct);

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct)
    {
        var active = await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedUtc == null)
            .ToListAsync(ct);

        foreach (var t in active)
            t.Revoke();
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => _context.SaveChangesAsync(ct);
}
