using SymptomTracker.Application.Interfaces;
using SymptomTracker.Domain.Entities;
using SymptomTracker.Domain.Enums;
using MathNet.Numerics.Statistics;

namespace SymptomTracker.Application.Services;

// computes Pearson correlation between atmospheric pressure and IIH symptom severity using nearest-neighbor within a tolerance window.
public class CorrelationService(
    ISymptomEntryRepository symptomEntryRepo,
    IEnvironmentSnapshotRepository environmentSnapshotRepo,
    ICorrelationResultRepository correlationResultRepo)
    : ICorrelationService
{
    // Pearson needs at least 5 pairs to be statistically meaningful, otherwise confidence in outcome is low (not enough data diversity)
    private const int MinimumPairsRequired = 5;

    public async Task<CorrelationResult> ComputeCorrelationAsync(DateTime windowStart, DateTime windowEnd, double toleranceHours, CancellationToken ct = default)
    {
        var symptoms = await symptomEntryRepo.GetByWindowAsync(windowStart, windowEnd, ct);
        var snapshots = await environmentSnapshotRepo.GetByWindowAsync(windowStart, windowEnd, ct);
        
        // exclude Caregiver entries - correlations only care about perceived symptoms from the patient
        var patientSymptoms = symptoms.Where(s => s.Role is EntryRole.Patient && s.OverallSeverity is not null).ToList();

        // tried shorter tolerances but the returned pairs didn't capture the spikes I was looking for
        var toleranceSpan = TimeSpan.FromHours(toleranceHours);
        var dataPairs = BuildPairs(patientSymptoms, snapshots, toleranceSpan);

        double? coefficient = null;
        CorrelationConfidence confidence;

        if (dataPairs.Count >= MinimumPairsRequired)
        {
            // if identical values are logged (no variance) both stdev values are 0, resulting in a divide-by-zero scenario (NaN)
            var raw = ComputePearson(dataPairs);
            coefficient = double.IsNaN(raw) || double.IsInfinity(raw) ? null : raw;
            
            // if Pearson returns NaN and coefficient is null, default to InsufficientData for confidence
            confidence = coefficient is null ? CorrelationConfidence.InsufficientData : ClassifyConfidence(dataPairs.Count);
        }
        else
            confidence = CorrelationConfidence.InsufficientData;

        var result = CorrelationResult.Create(
            windowStart: windowStart,
            windowEnd: windowEnd,
            symptomEntryCount: symptoms.Count,
            snapshotCount: snapshots.Count,
            pairedDataCount: dataPairs.Count,
            pressureSeverityCorrelation: coefficient,
            confidence: confidence,
            toleranceHours: toleranceHours);
        
        await correlationResultRepo.AddAsync(result, ct);
        await correlationResultRepo.SaveChangesAsync(ct);
        
        return result;
    }

    // find the snapshot with the smallest time distance for each symptom entry within given toleranceSpan
    private static List<(double Pressure, double Severity)> BuildPairs(
        List<SymptomEntry> symptoms,
        List<EnvironmentSnapshot> snapshots,
        TimeSpan toleranceSpan)
    {
        // TODO: better way to structure the data here? different type?
        var pairs = new List<(double, double)>();

        // O(n^2) time, good enough for small datasets - skipped sorting the data
        foreach (var symptom in symptoms)
        {
            if (!symptom.OverallSeverity.HasValue) continue;
            
            EnvironmentSnapshot? nearest = null;
            var minGap = TimeSpan.MaxValue;

            foreach (var snapshot in snapshots)
            {
                var gap = (symptom.Timestamp - snapshot.Timestamp).Duration();
                if (gap >= minGap) continue;
                minGap = gap;
                nearest = snapshot;
            }
            
            if (nearest is not null && minGap <= toleranceSpan)
                pairs.Add((nearest.PressureHpa, symptom.OverallSeverity.Value));
        }
        
        return pairs;
    }

    // find Pearson correlation coefficient computation between pressure readings and severity values
    // source: https://www.statology.org/pearson-correlation-coefficient/
    private static double ComputePearson(List<(double Pressure, double Severity)> dataPairs)
    {
        // MathNet.Numerics enumerates both lists under the hood - offloading the overhead here instead
        var seriesPressure = dataPairs.Select(p => p.Pressure).ToArray();
        var seriesSeverity = dataPairs.Select(p => p.Severity).ToArray();
        
        // used MathNet.Numerics instead of reinventing and breaking the calculation
        return Correlation.Pearson(seriesPressure, seriesSeverity);
    }

    private static CorrelationConfidence ClassifyConfidence(int dataPairCount) 
        => dataPairCount switch
    {
        < MinimumPairsRequired => CorrelationConfidence.InsufficientData,
        < 10 => CorrelationConfidence.Low,
        < 30 => CorrelationConfidence.Medium,
        _ => CorrelationConfidence.High
    };
}