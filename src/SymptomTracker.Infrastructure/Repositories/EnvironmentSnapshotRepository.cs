using Microsoft.EntityFrameworkCore;
using SymptomTracker.Application.Interfaces;
using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Infrastructure.Repositories;

public class EnvironmentSnapshotRepository(AppDbContext ctx) : IEnvironmentSnapshotRepository
{
    public Task<EnvironmentSnapshot?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => ctx.EnvironmentSnapshots.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<List<EnvironmentSnapshot>> GetByWindowAsync(DateTime start, DateTime end, CancellationToken ct = default)
        => ctx.EnvironmentSnapshots
            .Where(s => s.Timestamp >= start && s.Timestamp <= end)
            .OrderBy(s => s.Timestamp)
            .ToListAsync(ct);

    public Task<EnvironmentSnapshot?> GetMostRecentBeforeTimestampAsync(DateTime timestamp, CancellationToken ct = default)
        => ctx.EnvironmentSnapshots
            .Where(s => s.Timestamp < timestamp)
            .OrderByDescending(s => s.Timestamp)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(EnvironmentSnapshot snapshot, CancellationToken ct = default)
        => await ctx.EnvironmentSnapshots.AddAsync(snapshot, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => ctx.SaveChangesAsync(ct);
}