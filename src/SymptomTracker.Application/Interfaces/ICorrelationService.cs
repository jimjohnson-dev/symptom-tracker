using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Application.Interfaces;

public interface ICorrelationService
{
    /// <summary>
    /// Computes a Pearson correlation between barometric pressure and symptom severity given
    /// a time window. Returns a persisted result.
    /// </summary>
    Task<CorrelationResult> ComputeAsync(
        DateTime windowStart,
        DateTime windowEnd,
        double toleranceHours,
        CancellationToken cancellationToken = default);
}