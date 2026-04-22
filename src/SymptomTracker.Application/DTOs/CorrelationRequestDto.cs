using System.ComponentModel.DataAnnotations;
using SymptomTracker.Domain.Enums;

namespace SymptomTracker.Application.DTOs;

/// <summary>
/// Params for correlation computation requests
/// </summary>
public class CorrelationRequestDto
{
    /// <summary>
    /// Number of days included for historical analysis. Default to 1 week, max 3 months to capture seasonal symptom shifts
    /// </summary>
    [Range(1, 90, ErrorMessage = "Value must be between -90 and 90")]
    public int WindowDays { get; set; } = 7;
    
    /// <summary>
    /// Amount of buffer time around each symptom entry for matching snapshot search. Default to 2 hours, expect patients to log symptom changes
    /// in this window
    ///
    /// Tradeoffs: Tighter tolerance -> fewer pairs but higher accuracy, Looser tolerance -> more pairs but more room for mismatched results
    /// </summary>
    [Range(0.5, 24.0, ErrorMessage = "Value must be between 0.5 and 24.0")]
    public double ToleranceHours { get; set; } = 2.0;
}

public class CorrelationResultDto
{
    public Guid Id { get; set; }
    public DateTime ComputedAt { get; set; }
    public DateTime WindowStart { get; set; }
    public DateTime WindowEnd { get; set; }
    public int SymptomEntryCount { get; set; }
    public int SnapshotCount { get; set; }
    public int PairedDataCount { get; set; }
    public double? PressureSeverityCorrelation { get; set; }
    public CorrelationConfidence Confidence { get; set; }
    public double ToleranceHours { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// Plain-language interpretation of the correlation coefficient for UI display and primary use case (patient)
    /// </summary>
    public string Interpretation => BuildInterpretation();

    private string BuildInterpretation()
    {
        if (Confidence == CorrelationConfidence.InsufficientData || PressureSeverityCorrelation is null)
            return "Not enough data to determine pattern yet.";

        return PressureSeverityCorrelation switch
        {
            // Lower pressure expected to correlate with higher severity, as is common in IIH patients
            < -0.5 => "Symptoms tend to worsen when pressure drops.",
            
            // Strong positive correlation means high pressure is driving symptoms, can be a sign of other issues at play
            > 0.5 => "Symptoms tend to worsen when pressure rises.",
            
            // No meaningful linear relationship expected between -0.5 and 0.5
            _ => "Weak or no correlation detected in this window."
        };
    }
}