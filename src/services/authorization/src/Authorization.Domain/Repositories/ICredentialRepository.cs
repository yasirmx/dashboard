using Authorization.Domain.Entities;

namespace Authorization.Domain.Repositories;

public interface ICredentialRepository
{
    Task<Credential?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Credential?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(Credential credential, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
