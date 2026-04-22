using System.ComponentModel.DataAnnotations;

namespace SymptomTracker.Application.DTOs;

public static class PressureConversion
{
    // Conversion factor from 1 hPa to Inches of mercury (Hg) per SI units
    // source: https://www.convertunits.com/from/hpa/to/inhg
    public const double HpaToInHg = 0.02953;
}

public class CreateEnvironmentSnapshotRequest
{
    /// <summary>
    /// Lat/Long coordinates for fetching weather data from provider with no persistence.
    /// Opting to store City-State location instead for privacy concerns.
    /// </summary>
    [Required]
    [Range(-90, 90, ErrorMessage = "Value must be between -90 and 90")]
    public double Latitude { get; set; }
    
    [Required]
    [Range(-180, 180, ErrorMessage = "Value must be between -90 and 90")]
    public double Longitude { get; set; }
    
    /// <summary>
    /// Manual override options for testing to bypass Lat/Long if desired
    /// </summary>
    public double? ManualPressureHpa { get; set; }
    public double? ManualTemperatureFahrenheit { get; set; }
    public double? ManualHumidityPercent { get; set; }
    public string? ManualLocation { get; set; }
    
    public DateTime? Timestamp { get; set; }
}

public class EnvironmentSnapshotDto
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public double PressureHpa { get; set; }
    
    // Include pressure as in/Hg to match typical reporting metrics from US weather apps
    public double PressureInHg => Math.Round(PressureHpa * PressureConversion.HpaToInHg, 4);
    
    public double? Delta12HrHpa { get; set; }
    public double? Delta24HrHpa { get; set; }
    public double? TemperatureFahrenheit { get; set; }
    public double? HumidityPercent { get; set; }
    public string? Location { get; set; }
}