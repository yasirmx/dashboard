using User.Domain.Entities;

namespace User.Domain.Repositories;

public interface IProfileRepository
{
    Task<Profile?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Profile>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);
    Task<IReadOnlyList<Profile>> SearchAsync(string? search, string? role, CancellationToken ct = default);
    Task AddAsync(Profile profile, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
