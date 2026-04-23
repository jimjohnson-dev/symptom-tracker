using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Application.Interfaces;

public interface ICorrelationService
{
    // computes the Pearson correlation coefficient between barometric pressure and symptom severity in a given timespan
    Task<CorrelationResult> ComputeCorrelationAsync(DateTime windowStart, DateTime windowEnd, double toleranceHours, CancellationToken ct = default);
}