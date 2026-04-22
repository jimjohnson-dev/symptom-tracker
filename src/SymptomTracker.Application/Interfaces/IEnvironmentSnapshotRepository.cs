using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Application.Interfaces;

public interface IEnvironmentSnapshotRepository
{
    Task<EnvironmentSnapshot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<EnvironmentSnapshot>> GetByWindowAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Returns the snapshot with the most recent creation time before <param name="timestamp"></param>.
    /// Avoids table scanning by calculating pressure deltas at write time.
    /// </summary>
    Task<EnvironmentSnapshot?> GetMostRecentBeforeAsync(DateTime timestamp, CancellationToken cancellationToken = default);
    
    Task AddAsync(EnvironmentSnapshot snapshot, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}