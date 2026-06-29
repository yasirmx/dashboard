using Microsoft.EntityFrameworkCore;
using User.Domain.Entities;
using User.Domain.Repositories;
using User.Infrastructure.Persistence;

namespace User.Infrastructure.Persistence.Repositories;

public sealed class ProfileRepository : IProfileRepository
{
    private readonly UsersContext _context;

    public ProfileRepository(UsersContext context) => _context = context;

    public Task<Profile?> GetByIdAsync(Guid userId, CancellationToken ct)
        => _context.Profiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);

    public async Task<IReadOnlyList<Profile>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct)
    {
        var ids = userIds.Distinct().ToArray();
        return await _context.Profiles
            .Where(p => ids.Contains(p.UserId))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Profile>> SearchAsync(string? search, string? role, CancellationToken ct)
    {
        var query = _context.Profiles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                EF.Functions.Like(p.FirstName, $"%{term}%") ||
                EF.Functions.Like(p.LastName, $"%{term}%") ||
                EF.Functions.Like(p.Email, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var r = role.Trim();
            var userIds = _context.UserRoles.Where(ur => ur.Role == r).Select(ur => ur.UserId);
            query = query.Where(p => userIds.Contains(p.UserId));
        }

        return await query.OrderBy(p => p.FirstName).ThenBy(p => p.LastName).ToListAsync(ct);
    }

    public async Task AddAsync(Profile profile, CancellationToken ct)
        => await _context.Profiles.AddAsync(profile, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _context.SaveChangesAsync(ct);
}
