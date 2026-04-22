namespace SymptomTracker.Domain.Entities;

/// <summary>
/// Captures time-bound barometric pressure changes 
/// </summary>
public sealed class EnvironmentSnapshot
{
    public Guid Id { get; private set; }
    public DateTime Timestamp { get; private set; }
    
    /// <summary>
    /// Barometric pressure in hectopascals (hPa) to match SI unit returned by weather APIs
    /// Intracranial Pressure (ICP) typically measured in mmMg, conversion handled by DTO layer
    /// Standard sea-level pressure = 1013.25 hPa, assume sea-level for v1
    /// </summary>
    public double PressureHpa { get; private set; }
    
    /// <summary>
    /// Track pressure change over previous 12 hours. Rapid 12-hr drops >= 5 hPa commonly correlate
    /// to migraines/IIH symptom increases.
    /// </summary>
    public double? Delta12HrHpa { get; private set; }
    
    /// <summary>
    /// Track pressure change over previous 24 hours. Normalizes bursty noise over shorter time frames.
    /// </summary>
    public double? Delta24HrHpa { get; private set; }
    public double? TemperatureFahrenheit { get; private set; }
    public double? HumidityPercent { get; private set; }
    
    /// <summary>
    /// Label for current location (City, State) for v1 visuals only. Removes privacy and tracking concerns.
    /// </summary>
    public string? Location { get; private set; }

    private EnvironmentSnapshot() { }
    
    public static EnvironmentSnapshot Create(
        double pressureHpa,
        double? delta12HrHpa = null,
        double? delta24HrHpa = null,
        double? temperatureFahrenheit = null,
        double? humidityPercent = null,
        string? location = null,
        DateTime? timestamp = null)
    {
        return new EnvironmentSnapshot
        {
            Id = Guid.NewGuid(),
            Timestamp = timestamp?.ToUniversalTime() ?? DateTime.UtcNow,
            PressureHpa = pressureHpa,
            Delta12HrHpa = delta12HrHpa,
            Delta24HrHpa = delta24HrHpa,
            TemperatureFahrenheit = temperatureFahrenheit,
            HumidityPercent = humidityPercent,
            Location = location
        };
    }
}