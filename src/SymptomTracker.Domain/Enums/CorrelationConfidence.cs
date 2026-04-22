namespace SymptomTracker.Domain.Enums;

/// <summary>
/// Categorical confidence measurement of calculated correlation
/// </summary>
public enum CorrelationConfidence
{
    Low,
    Medium,
    High,
    InsufficientData,
    Unknown
}