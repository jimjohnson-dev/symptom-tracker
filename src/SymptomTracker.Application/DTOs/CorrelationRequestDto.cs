using System.ComponentModel.DataAnnotations;
using SymptomTracker.Domain.Enums;

namespace SymptomTracker.Application.DTOs;

public class CorrelationRequestDto
{
    // TODO: use consts instead of hardcoding vals here if range vals change a lot
    // days to include in analysis, 3 months can catch seasonal weather changes
    [Range(1, 90, ErrorMessage = "Value must be between -90 and 90")]
    public int WindowDays { get; set; } = 7;
    
    // amount of buffer time around each symptom entry for matching snapshot search. Tighter tolerance -> fewer pairs but higher accuracy
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
    public double? PressureSeverityCorrelations { get; set; }
    public CorrelationConfidence Confidence { get; set; }
    public double ToleranceHours { get; set; }
    public string? Notes { get; set; }


    // TODO: return user-friendly wording for results?
    public string Interpretation => BuildInterpretation();

    private string BuildInterpretation()
    {
        if (Confidence == CorrelationConfidence.InsufficientData || PressureSeverityCorrelations is null)
            return "Not enough data to determine pattern yet.";

        return PressureSeverityCorrelations switch
        {
            // lower pressure expected to correlate with higher severity (common in IIH patients)
            < -0.5 => "Symptoms tend to worsen when pressure drops.",
            
            // high value = high pressure causing issues, not uncommon but usually points to a shunt obstruction, failure, or infection
            > 0.5 => "Symptoms tend to worsen when pressure rises.",
            
            // not enough variance to be a meaningful correlation in this range
            _ => "Weak or no correlation detected in this window."
        };
    }
}