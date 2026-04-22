namespace SymptomTracker.Domain.Entities;

/// <summary>
/// Persistant snapshot of pressure-to-symptom correlation over a time window. Correlations computed at
/// write time to avoid rerunning the calculation to retrieve past analyses for trending ops. Also keeps
/// the data auditable and idempotent. 
/// </summary>
public sealed class CorrelationResult
{
    // TODO: Add Ids, timestamps, Pearson coefficient fields, ctors
}