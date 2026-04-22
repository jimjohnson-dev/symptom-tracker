namespace SymptomTracker.Application.Interfaces;

/// <summary>
/// Weather data retrieval stub in place of actual Weather API, to be replaced by Open-Meteo, OpenWeatherMap, etc
/// </summary>
public interface IWeatherDataProvider
{
    Task<WeatherReading> GetCurrentReadingAsync(double lat, double lon, CancellationToken cancellationToken = default);
}

/// <summary>
/// Value object returned by the weather provider stub as record for idempotent testing
/// </summary>
public record WeatherReading(
    double PressureHpa,
    double? TemperatureFahrenheit,
    double? HumidityPercent,
    string? Location);