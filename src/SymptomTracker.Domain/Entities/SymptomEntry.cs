using SymptomTracker.Domain.Enums;

namespace SymptomTracker.Domain.Entities;

/// <summary>
/// Records each symptom observation of Patient as a record, to be used by Patient (v1) and Caregiver (v2)
/// </summary>
public sealed class SymptomEntry
{
    public Guid Id { get; private set; }
    public DateTime Timestamp { get; private set; }
    public EntryRole Role { get; private set; }
    
    /// <summary>
    /// Self-reported scale of 0 (none) to 10 (debilitating/severe/etc)
    /// </summary>
    public double? HeadPainLevel { get; private set; }
    
    /// <summary>
    /// Self-reported scale of 0 (none) to 10 (debilitating/severe/etc)
    /// </summary>
    public double? EyePressure { get; private set; }
    
    /// <summary>
    /// Self-reported vision clarity - 0 = very blurry, 10 = fully clear
    /// </summary>
    public double? VisionClarity { get; private set; }
    
    /// <summary>
    /// Self-reported scale of 0 (none) to 10 (debilitating/severe/etc)
    /// </summary>
    public double? Fatigue { get; private set; }
    
    /// <summary>
    /// Self-reported scale of 0 (none) to 10 (debilitating/severe/etc)
    /// </summary>
    public double? Nausea { get; private set; }
    
    /// <summary>
    /// Self-reported scale of 0 (none) to 10 (debilitating/severe/etc)
    /// </summary>
    public double? Aphasia { get; private set; }
    /// <summary>
    /// Self-reported scale of 0 (none) to 10 (debilitating/severe/etc)
    /// </summary>
    public double? Confusion { get; private set; }
    
    /// <summary>
    /// Open-ended observation notes available for Patient or Caregiver entry
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Average of symptom values for a given entry, null if no values added (notes only)
    /// </summary>
    public double? OverallSeverity
    {
        get
        {
            var symptomVals = new []{ HeadPainLevel, EyePressure, VisionClarity, Fatigue, Nausea, Aphasia, Confusion }.OfType<double>().ToArray();
            return symptomVals.Length == 0 ? null : symptomVals.Average();
        }
    }
    
    private SymptomEntry() { }
    
    public static SymptomEntry Create(
        EntryRole role, 
        DateTime? timestamp = null,
        double? headPainLevel = null,
        double? eyePressure = null,
        double? visionClarity = null,
        double? fatigue = null,
        double? nausea = null,
        double? aphasia = null,
        double? confusion = null,
        string? notes = null)
    {
        return new SymptomEntry
        {
            Id = Guid.NewGuid(),
            Timestamp = timestamp?.ToUniversalTime() ?? DateTime.UtcNow,
            Role = role,
            HeadPainLevel = headPainLevel,
            EyePressure = eyePressure,
            VisionClarity = visionClarity,
            Fatigue = fatigue,
            Nausea = nausea,
            Aphasia = aphasia,
            Confusion = confusion,
            Notes = notes
        };
    }
}