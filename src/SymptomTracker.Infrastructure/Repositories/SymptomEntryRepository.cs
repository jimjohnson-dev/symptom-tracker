using Microsoft.EntityFrameworkCore;
using SymptomTracker.Application.Interfaces;
using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Infrastructure.Repositories;

public class SymptomEntryRepository(AppDbContext context) : ISymptomEntryRepository
{
    public Task<SymptomEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => context.SymptomEntries.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<List<SymptomEntry>> GetByWindowAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
        => context.SymptomEntries
            .Where(e => e.Timestamp >= start && e.Timestamp <= end)
            .OrderBy(e => e.Timestamp)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(SymptomEntry entry, CancellationToken cancellationToken = default)
        => await context.SymptomEntries.AddAsync(entry, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}