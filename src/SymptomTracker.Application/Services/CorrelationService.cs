using SymptomTracker.Application.Interfaces;
using SymptomTracker.Domain.Entities;
using SymptomTracker.Domain.Enums;
using MathNet.Numerics.Statistics;

namespace SymptomTracker.Application.Services;

/// <summary>
/// Computes Pearson correlation between atmospheric pressure and IIH symptom severity using nearest-neighbor within a tolerance window.
/// </summary>
public class CorrelationService(
    ISymptomEntryRepository symptomEntryRepository,
    IEnvironmentSnapshotRepository environmentSnapshotRepository,
    ICorrelationResultRepository correlationResultRepository)
    : ICorrelationService
{
    // Less than 5 pairs makes Pearson statistically insignificant. Do not report any coefficient below this threshold
    private const int MinimumPairsRequired = 5;

    public async Task<CorrelationResult> ComputeAsync(
        DateTime windowStart, 
        DateTime windowEnd, 
        double toleranceHours,
        CancellationToken cancellationToken = default)
    {
        // Get symptoms and snapshots for given time window
        var symptoms = await symptomEntryRepository.GetByWindowAsync(windowStart, windowEnd, cancellationToken);
        var snapshots = await environmentSnapshotRepository.GetByWindowAsync(windowStart, windowEnd, cancellationToken);
        
        // Exclude EntryRole.Caregiver - correlations only care about perceived symptoms from the patient
        var patientSymptoms = symptoms.Where(s => s is { Role: EntryRole.Patient, OverallSeverity: not null }).ToList();

        var toleranceSpan = TimeSpan.FromHours(toleranceHours);
        var pairs = BuildPairs(patientSymptoms, snapshots, toleranceSpan);

        double? coefficient = null;
        CorrelationConfidence confidence;

        if (pairs.Count >= MinimumPairsRequired)
        {
            coefficient = ComputePearson(pairs);
            confidence = ClassifyConfidence(pairs.Count);
        }
        else
        {
            confidence = CorrelationConfidence.InsufficientData;
        }

        var result = CorrelationResult.Create(
            windowStart: windowStart,
            windowEnd: windowEnd,
            symptomEntryCount: symptoms.Count,
            snapshotCount: snapshots.Count,
            pairedDataCount: pairs.Count,
            pressureSeverityCorrelation: coefficient,
            confidence: confidence,
            toleranceHours: toleranceHours);
        
        await correlationResultRepository.AddAsync(result, cancellationToken);
        await correlationResultRepository.SaveChangesAsync(cancellationToken);
        
        return result;
    }

    /// <summary>
    /// Find the snapshot with the smallest time distance for each patient symptom entry, excluding entries where the distance exceeds the toleranceSpan.
    /// </summary>
    private static List<(double Pressure, double Severity)> BuildPairs(
        List<SymptomEntry> symptoms,
        List<EnvironmentSnapshot> snapshots,
        TimeSpan toleranceSpan)
    {
        var pairs = new List<(double, double)>();

        foreach (var symptom in symptoms)
        {
            if (!symptom.OverallSeverity.HasValue) continue;
            
            EnvironmentSnapshot? nearest = null;
            var minGap = TimeSpan.MaxValue;

            foreach (var pair in snapshots)
            {
                var gap = (symptom.Timestamp - pair.Timestamp).Duration();
                if (gap >= minGap) continue;
                minGap = gap;
                nearest = pair;
            }
            
            if (nearest is not null && minGap <= toleranceSpan)
                pairs.Add((nearest.PressureHpa, symptom.OverallSeverity.Value));
        }
        
        return pairs;
    }

    /// <summary>
    /// Pearson correlation coefficient computation given pressure readings and severity values
    /// source: https://www.statology.org/pearson-correlation-coefficient/
    /// </summary>
    private static double ComputePearson(List<(double Pressure, double Severity)> pairs)
    {
        var seriesPressure = pairs.Select(p => p.Pressure).Where(p => !double.IsNaN(p)).ToArray();
        var seriesSeverity = pairs.Select(p => p.Severity).Where(p => !double.IsNaN(p)).ToArray();
        
        // Use MathNet.Numerics instead of reinventing the solution
        var correlation = Correlation.Pearson(seriesPressure, seriesSeverity);
        return correlation;
    }

    private static CorrelationConfidence ClassifyConfidence(int pairCount) 
        => pairCount switch
    {
        < MinimumPairsRequired => CorrelationConfidence.InsufficientData,
        < 10 => CorrelationConfidence.Low,
        < 30 => CorrelationConfidence.Medium,
        _ => CorrelationConfidence.High
    };
}