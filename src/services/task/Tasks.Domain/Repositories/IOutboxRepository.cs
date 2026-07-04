using Tasks.Domain.Entities;

namespace Tasks.Domain.Repositories;

public interface IOutboxRepository
{
    System.Threading.Tasks.Task AddAsync(OutboxMessage message, CancellationToken ct = default);
    System.Threading.Tasks.Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken ct = default);
    System.Threading.Tasks.Task SaveChangesAsync(CancellationToken ct = default);
}
