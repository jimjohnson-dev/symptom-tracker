using SymptomTracker.Application.Interfaces;

namespace SymptomTracker.Infrastructure.Providers;

/// <summary>
/// STUB: Returns simulated weather data
///
/// To implement a real weather provider, register a new class that inherits IWeatherDataProvider with
/// API key in user secrets or env vars. Recommend against using appsettings.json even at this scale.
/// </summary>
public class StubWeatherDataProvider : IWeatherDataProvider
{
    // ICAO standard atmosphere at sea level, source: https://www.flyeye.io/drone-acronym-isa/
    private const double StandardPressureHpa = 1013.25;
    private const double PressureVarianceHpa = 10.0;
    private const double StandardTempFahrenheit = 59.0;
    private const double StandardHumidityPercent = 55.0;

    private readonly Random _rand = new();
    
    public Task<WeatherReading> GetCurrentReadingAsync(double lat, double lon, CancellationToken cancellationToken = default)
    {
        // Simulated mid-latitude weather variance for testing purposes
        var pressure = StandardPressureHpa + (_rand.NextDouble() * PressureVarianceHpa * 2 - PressureVarianceHpa);
        var temp = StandardTempFahrenheit + (_rand.NextDouble() * 10 - 5);
        var humidity = StandardHumidityPercent + (_rand.NextDouble() * 30 - 15);
        
        // Mock location from coords, replace with actual city name from API response when implemented
        var location = $@"~{Math.Round(lat, 1)}\u00B0, {Math.Round(lon, 1)}\u00B0F (stub)";

        var reading = new WeatherReading(
            PressureHpa: Math.Round(pressure, 2),
            TemperatureFahrenheit: Math.Round(temp, 1),
            HumidityPercent: Math.Round(humidity, 1),
            Location: location);

        return Task.FromResult(reading);
    }
}