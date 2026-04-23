using SymptomTracker.Domain.Enums;

namespace SymptomTracker.Domain.Entities;

public sealed class CorrelationResult
{
    public Guid Id { get; private set; }
    
    public DateTime ComputedAt { get; private set; }
    public DateTime WindowStart { get; private set; }
    public DateTime WindowEnd { get; private set; }
    public int SymptomEntryCount { get; private set; }
    public int SnapshotCount { get; private set; }
    
    // symptom-environmentSnapshot data pairs for correlation calcs
    public int PairedDataCount { get; private set; }
    
    /// <summary>
    /// Pearson correlation coefficient between barometric pressure value and overall symptom severity.
    /// Scale range: [-1, 1]
    ///     -1 = perfect inverse correlation, lower pressure = higher severity expected for IIH patients
    ///      0 = no correlation
    ///     +1 = perfect positive correlation, higher pressure = higher severity (possible VP shunt failure, infection, etc)
    /// Null when PairedDataCount below minimum threshold (5)
    /// </summary>
    public double? PressureSeverityCorrelations { get; private set; }
    public CorrelationConfidence Confidence { get; private set; }
    
    // maximum time gap allowed between entries to find nearest neighbor snapshot
    public double ToleranceHours { get; private set; }
    public string? Notes { get; private set; }
    
    private CorrelationResult() { }
    
    public static CorrelationResult Create(
        DateTime windowStart,
        DateTime windowEnd,
        int symptomEntryCount,
        int snapshotCount,
        int pairedDataCount,
        double? pressureSeverityCorrelation,
        CorrelationConfidence confidence,
        double toleranceHours,
        string? notes = null)
    {
        return new CorrelationResult
        {
            Id = Guid.NewGuid(),
            ComputedAt = DateTime.UtcNow,
            WindowStart = windowStart,
            WindowEnd = windowEnd,
            SymptomEntryCount = symptomEntryCount,
            SnapshotCount = snapshotCount,
            PairedDataCount = pairedDataCount,
            PressureSeverityCorrelations = pressureSeverityCorrelation,
            Confidence = confidence,
            ToleranceHours = toleranceHours,
            Notes = notes
        };
    }
}