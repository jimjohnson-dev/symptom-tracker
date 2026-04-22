using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Application.Interfaces;

public interface ISymptomEntryRepository
{
    Task<SymptomEntry?> GetByIdAsync(Guid id,  CancellationToken cancellationToken = default);
    Task<List<SymptomEntry>> GetByWindowAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task AddAsync(SymptomEntry entry, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}