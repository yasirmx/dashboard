using Authorization.Domain.Entities;
using Authorization.Domain.Repositories;
using Authorization.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Infrastructure.Persistence.Repositories;

public sealed class UserRoleRepository : IUserRoleRepository
{
    private readonly AuthorizationContext _context;

    public UserRoleRepository(AuthorizationContext context) => _context = context;

    public async Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken ct)
        => await _context.UserRoles
            .Where(r => r.UserId == userId)
            .Select(r => r.Role)
            .ToListAsync(ct);

    public async Task ReplaceRolesAsync(Guid userId, IEnumerable<string> roles, CancellationToken ct)
    {
        var existing = await _context.UserRoles
            .Where(r => r.UserId == userId)
            .ToListAsync(ct);

        _context.UserRoles.RemoveRange(existing);

        foreach (var role in roles)
            await _context.UserRoles.AddAsync(UserRole.Create(userId, role), ct);
    }

    public Task SaveChangesAsync(CancellationToken ct)
        => _context.SaveChangesAsync(ct);
}
