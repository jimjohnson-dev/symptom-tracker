using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Application.Interfaces;

public interface ICorrelationResultRepository
{
    Task<CorrelationResult?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(CorrelationResult result, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    // TODO: add GetByWindowAsync(...) and related controller GET
}