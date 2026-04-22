using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Application.Interfaces;

public interface ICorrelationResultRepository
{
    Task<CorrelationResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(CorrelationResult result, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}