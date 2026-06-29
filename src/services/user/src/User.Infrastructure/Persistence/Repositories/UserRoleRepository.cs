using Microsoft.EntityFrameworkCore;
using User.Domain.Entities;
using User.Domain.Repositories;
using User.Infrastructure.Persistence;

namespace User.Infrastructure.Persistence.Repositories;

public sealed class UserRoleRepository : IUserRoleRepository
{
    private readonly UsersContext _context;

    public UserRoleRepository(UsersContext context) => _context = context;

    public async Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken ct)
        => await _context.UserRoles
            .Where(r => r.UserId == userId)
            .Select(r => r.Role)
            .ToListAsync(ct);

    public async Task<IReadOnlyDictionary<Guid, string[]>> GetRolesAsync(
        IEnumerable<Guid> userIds, CancellationToken ct)
    {
        var ids = userIds.Distinct().ToArray();

        var rows = await _context.UserRoles
            .Where(r => ids.Contains(r.UserId))
            .ToListAsync(ct);

        return rows
            .GroupBy(r => r.UserId)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Role).ToArray());
    }

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
