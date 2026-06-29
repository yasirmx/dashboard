using Authorization.Domain.Entities;

namespace Authorization.Domain.Repositories;

public interface IUserRoleRepository
{
    Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken ct = default);
    Task ReplaceRolesAsync(Guid userId, IEnumerable<string> roles, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
