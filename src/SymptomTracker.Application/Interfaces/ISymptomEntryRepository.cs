using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Application.Interfaces;

public interface ISymptomEntryRepository
{
    Task<SymptomEntry?> GetByIdAsync(Guid id,  CancellationToken ct = default);
    Task<List<SymptomEntry>> GetByWindowAsync(DateTime start, DateTime end, CancellationToken ct = default);
    Task AddAsync(SymptomEntry entry, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}