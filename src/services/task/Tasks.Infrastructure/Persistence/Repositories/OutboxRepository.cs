using Tasks.Domain.Entities;
using Tasks.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Tasks.Infrastructure.Persistence.Repositories;

public sealed class OutboxRepository : IOutboxRepository
{
    private readonly TaskContext _context;

    public OutboxRepository(TaskContext context) => _context = context;

    public async System.Threading.Tasks.Task AddAsync(OutboxMessage message, CancellationToken ct)
        => await _context.OutboxMessages.AddAsync(message, ct);

    public async System.Threading.Tasks.Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken ct)
        => await _context.OutboxMessages
            .Where(m => m.ProcessedUtc == null)
            .OrderBy(m => m.OccurredUtc)
            .Take(batchSize)
            .ToListAsync(ct);

    public System.Threading.Tasks.Task SaveChangesAsync(CancellationToken ct)
        => _context.SaveChangesAsync(ct);
}
