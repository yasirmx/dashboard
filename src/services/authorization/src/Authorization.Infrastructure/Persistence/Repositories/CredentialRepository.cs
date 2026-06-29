using Authorization.Domain.Entities;
using Authorization.Domain.Repositories;
using Authorization.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Infrastructure.Persistence.Repositories;

public sealed class CredentialRepository : ICredentialRepository
{
    private readonly AuthorizationContext _context;

    public CredentialRepository(AuthorizationContext context) => _context = context;

    public Task<Credential?> GetByEmailAsync(string email, CancellationToken ct)
        => _context.Credentials
            .FirstOrDefaultAsync(c => c.Email == email.ToLowerInvariant(), ct);

    public Task<Credential?> GetByIdAsync(Guid id, CancellationToken ct)
        => _context.Credentials.FindAsync([id], ct).AsTask();

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct)
        => _context.Credentials
            .AnyAsync(c => c.Email == email.ToLowerInvariant(), ct);

    public async Task AddAsync(Credential credential, CancellationToken ct)
        => await _context.Credentials.AddAsync(credential, ct);

    public Task SaveChangesAsync(CancellationToken ct)
        => _context.SaveChangesAsync(ct);
}
