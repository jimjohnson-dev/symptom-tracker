using Microsoft.EntityFrameworkCore;
using SymptomTracker.Application.Interfaces;
using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Infrastructure.Repositories;

public class SymptomEntryRepository(AppDbContext ctx) : ISymptomEntryRepository
{
    public Task<SymptomEntry?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => ctx.SymptomEntries.FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<List<SymptomEntry>> GetByWindowAsync(DateTime start, DateTime end, CancellationToken ct = default)
        => ctx.SymptomEntries
            .Where(e => e.Timestamp >= start && e.Timestamp <= end)
            .OrderBy(e => e.Timestamp)
            .ToListAsync(ct);

    public async Task AddAsync(SymptomEntry entry, CancellationToken ct = default)
        => await ctx.SymptomEntries.AddAsync(entry, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => ctx.SaveChangesAsync(ct);
}