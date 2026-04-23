namespace SymptomTracker.Application.Interfaces;

// stub in place of actual Weather API, to be replaced by Open-Meteo, OpenWeatherMap, etc
public interface IWeatherDataProvider
{
    Task<WeatherReading> GetCurrentReadingAsync(double lat, double lon, CancellationToken ct = default);
}

public record WeatherReading(double PressureHpa, double? TemperatureFahrenheit, double? HumidityPercent, string? Location);