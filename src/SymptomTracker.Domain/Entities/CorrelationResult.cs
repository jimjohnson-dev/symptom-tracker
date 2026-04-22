using SymptomTracker.Domain.Enums;

namespace SymptomTracker.Domain.Entities;

/// <summary>
/// Persistant snapshot of pressure-to-symptom correlation over a time window. Correlations computed at
/// write time to avoid rerunning the calculation to retrieve past analyses for trending ops. Also keeps
/// the data auditable and idempotent. 
/// </summary>
public sealed class CorrelationResult
{
    public Guid Id { get; private set; }
    
    /// <summary>
    /// Track time of correlation computation for auditing purposes
    /// </summary>
    public DateTime ComputedAt { get; private set; }
    public DateTime WindowStart { get; private set; }
    public DateTime WindowEnd { get; private set; }
    public int SymptomCount { get; private set; }
    public int SnapshotCount { get; private set; }
    
    /// <summary>
    /// Count of symptom entry/snapshot pairs matched within ToleranceHours, required for Pearson formula.
    /// Count less than SymptomCount when some entries have no nearby snapshot or were outside the tolerance
    /// window.
    /// </summary>
    public int PairedDataCount { get; private set; }
    
    /// <summary>
    /// Pearson correlation coefficient between barometric pressure value and overall symptom severity.
    /// Scale range: [-1, 1]
    ///     -1 = perfect inverse correlation, lower pressure = higher severity expected for IIH patients
    ///      0 = no correlation
    ///     +1 = perfect positive correlation, higher pressure = higher severity (possible VP shunt failure, infection, etc)
    /// Null when PairedDataCount below minimum threshold (5)
    /// </summary>
    public double? PressureSeverityCorrelation { get; private set; }
    public CorrelationConfidence Confidence { get; private set; }
    
    /// <summary>
    /// Maximum time gap (hours) allowed when pairing a symptom entry with its nearest neighbor snapshot.
    /// Stored with the result for idempotent ops because changing the tolerance can produce
    /// different pair counts.
    /// </summary>
    public double ToleranceHours { get; private set; }
    public string? Notes { get; private set; }
    
    private CorrelationResult() { }
    
    public static CorrelationResult Create(
        DateTime windowStart,
        DateTime windowEnd,
        int symptomCount,
        int snapshotCount,
        int pairedDataCount,
        double? pressureSeverityCorrelation,
        CorrelationConfidence confidence,
        double toleranceHours,
        string? notes)
    {
        return new CorrelationResult
        {
            Id = Guid.NewGuid(),
            ComputedAt = DateTime.UtcNow,
            WindowStart = windowStart,
            WindowEnd = windowEnd,
            SymptomCount = symptomCount,
            SnapshotCount = snapshotCount,
            PairedDataCount = pairedDataCount,
            PressureSeverityCorrelation = pressureSeverityCorrelation,
            Confidence = confidence,
            ToleranceHours = toleranceHours,
            Notes = notes
        };
    }
}