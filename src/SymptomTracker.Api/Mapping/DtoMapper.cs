using SymptomTracker.Application.DTOs;
using SymptomTracker.Domain.Entities;
using SymptomTracker.Domain.Enums;

namespace SymptomTracker.Api.Mapping;

// chose explicit mapping for this stage of the project, AutoMapper was overkill
public static class DtoMapper
{
    public static SymptomEntryDto ToDto(SymptomEntry e) => new()
    {
        Id = e.Id,
        Timestamp = e.Timestamp,
        Role = e.Role,
        HeadPainLevel = e.HeadPainLevel,
        EyePressure = e.EyePressure,
        VisionClarity = e.VisionClarity,
        Fatigue = e.Fatigue,
        Nausea = e.Nausea,
        Aphasia = e.Aphasia,
        Confusion = e.Confusion,
        Notes = e.Notes,
        OverallSeverity = e.OverallSeverity,
    };

    public static EnvironmentSnapshotDto ToDto(EnvironmentSnapshot s) => new()
    {
        Id = s.Id,
        Timestamp = s.Timestamp,
        PressureHpa = s.PressureHpa,
        Delta12HrHpa = s.Delta12HrHpa,
        Delta24HrHpa = s.Delta24HrHpa,
        TemperatureFahrenheit = s.TemperatureFahrenheit,
        HumidityPercent = s.HumidityPercent,
        Location = s.Location,
    };

    public static CorrelationResultDto ToDto(CorrelationResult r) => new()
    {
        Id = r.Id,
        ComputedAt = r.ComputedAt,
        WindowStart = r.WindowStart,
        WindowEnd = r.WindowEnd,
        SymptomEntryCount = r.SymptomEntryCount,
        SnapshotCount = r.SnapshotCount,
        PairedDataCount = r.PairedDataCount,
        PressureSeverityCorrelations = r.PressureSeverityCorrelations,
        Confidence = r.Confidence,
        ToleranceHours = r.ToleranceHours,
        Notes = r.Notes,
    };
}