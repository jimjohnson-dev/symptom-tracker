using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Application.Interfaces;

public interface IEnvironmentSnapshotRepository
{
    Task<EnvironmentSnapshot?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<EnvironmentSnapshot>> GetByWindowAsync(DateTime start, DateTime end, CancellationToken ct = default);
    
    // avoids table scanning by calculating pressure deltas at write time
    Task<EnvironmentSnapshot?> GetMostRecentBeforeTimestampAsync(DateTime timestamp, CancellationToken ct = default);
    
    Task AddAsync(EnvironmentSnapshot snapshot, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}