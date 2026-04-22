using System.ComponentModel.DataAnnotations;
using SymptomTracker.Domain.Enums;

namespace SymptomTracker.Application.DTOs;

/// <summary>
/// Input for creating a new symptom entry - all symptom values are optional to reduce user friction
/// </summary>
public class CreateSymptomEntryRequest
{
    public EntryRole Role { get; set; } = EntryRole.Patient;
    
    public DateTime? Timestamp { get; set; }
    
    [Range(0, 10, ErrorMessage = "Value must be between 0 and 10.")]
    public double? HeadPainLevel { get; set; }
    
    [Range(0, 10, ErrorMessage = "Value must be between 0 and 10.")]
    public double? EyePressure { get; set; }
    
    [Range(0, 10, ErrorMessage = "Value must be between 0 and 10.")]
    public double? VisionClarity { get; set; }
    
    [Range(0, 10, ErrorMessage = "Value must be between 0 and 10.")]
    public double? Fatigue { get; set; }
    
    [Range(0, 10, ErrorMessage = "Value must be between 0 and 10.")]
    public double? Nausea { get; set; }
    
    [Range(0, 10, ErrorMessage = "Value must be between 0 and 10.")]
    public double? Aphasia { get; set; }

    [Range(0, 10, ErrorMessage = "Value must be between 0 and 10.")]
    public double? Confusion { get; set; }
    
    [MaxLength(2000)]
    public string? Notes { get; set; }
}

public class SymptomEntryDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public EntryRole Role { get; set; }
    public double? HeadPainLevel { get; set; }
    public double? EyePressure { get; set; }
    public double? VisionClarity { get; set; }
    public double? Fatigue { get; set; }
    public double? Nausea { get; set; }
    public double? Aphasia { get; set; }
    public double? Confusion { get; set; }
    public string? Notes { get; set; }
    public double? AverageSeverity { get; set; }
}