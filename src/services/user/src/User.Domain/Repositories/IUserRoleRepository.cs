namespace User.Domain.Repositories;

public interface IUserRoleRepository
{
    Task<IReadOnlyList<string>> GetRolesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, string[]>> GetRolesAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);
    Task ReplaceRolesAsync(Guid userId, IEnumerable<string> roles, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
