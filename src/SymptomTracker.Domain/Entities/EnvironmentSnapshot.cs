namespace SymptomTracker.Domain.Entities;

public sealed class EnvironmentSnapshot
{
    public Guid Id { get; private set; }
    public DateTime Timestamp { get; private set; }
    
    // barometric pressure in hectopascals (hPa) to match SI unit returned by weather APIs
    public double PressureHpa { get; private set; }
    public double? Delta12HrHpa { get; private set; }
    public double? Delta24HrHpa { get; private set; }
    public double? TemperatureFahrenheit { get; private set; }
    public double? HumidityPercent { get; private set; }
    
    // city-state location text
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