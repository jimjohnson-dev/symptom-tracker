using System.ComponentModel.DataAnnotations;
using SymptomTracker.Domain.Enums;

namespace SymptomTracker.Application.DTOs;

// symptom entry vals optional for notes-only use case from Caregiver or Patient
public class CreateSymptomEntryRequest
{
    private const string ErrorMsg0To10 = "Value must be between 0 and 10.";
    private const int MaxNotesLength = 2000;
    private const int Min = 0, Max = 10;
    // only concerned with Patient for this version
    public EntryRole Role { get; set; } = EntryRole.Patient;
    
    public DateTime? Timestamp { get; set; }
    [Range(Min, Max, ErrorMessage = ErrorMsg0To10)]
    public double? HeadPainLevel { get; set; }
    
    [Range(Min, Max, ErrorMessage = ErrorMsg0To10)]
    public double? EyePressure { get; set; }
    
    [Range(Min, Max, ErrorMessage = ErrorMsg0To10)]
    public double? VisionClarity { get; set; }
    
    [Range(Min, Max, ErrorMessage = ErrorMsg0To10)]
    public double? Fatigue { get; set; }
    
    [Range(Min, Max, ErrorMessage = ErrorMsg0To10)]
    public double? Nausea { get; set; }
    
    [Range(Min, Max, ErrorMessage = ErrorMsg0To10)]
    public double? Aphasia { get; set; }

    [Range(Min, Max, ErrorMessage = ErrorMsg0To10)]
    public double? Confusion { get; set; }
    
    [MaxLength(MaxNotesLength)]
    public string? Notes { get; set; }
}

public class SymptomEntryDto
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public EntryRole Role { get; set; }
    public double? HeadPainLevel { get; set; }
    public double? EyePressure { get; set; }
    public double? VisionClarity { get; set; }
    public double? Fatigue { get; set; }
    public double? Nausea { get; set; }
    public double? Aphasia { get; set; }
    public double? Confusion { get; set; }
    public string? Notes { get; set; }
    public double? OverallSeverity { get; set; }
}