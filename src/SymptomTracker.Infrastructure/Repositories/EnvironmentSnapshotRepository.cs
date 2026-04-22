using Microsoft.EntityFrameworkCore;
using SymptomTracker.Application.Interfaces;
using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Infrastructure.Repositories;

public class EnvironmentSnapshotRepository(AppDbContext context) : IEnvironmentSnapshotRepository
{
    public Task<EnvironmentSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.EnvironmentSnapshots.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<List<EnvironmentSnapshot>> GetByWindowAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
        => context.EnvironmentSnapshots
            .Where(s => s.Timestamp >= start && s.Timestamp <= end)
            .OrderBy(s => s.Timestamp)
            .ToListAsync(cancellationToken);

    public Task<EnvironmentSnapshot?> GetMostRecentBeforeAsync(DateTime timestamp, CancellationToken cancellationToken = default)
        => context.EnvironmentSnapshots
            .Where(s => s.Timestamp < timestamp)
            .OrderByDescending(s => s.Timestamp)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(EnvironmentSnapshot snapshot, CancellationToken cancellationToken = default)
        => await context.EnvironmentSnapshots.AddAsync(snapshot, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}