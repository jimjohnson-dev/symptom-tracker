using Microsoft.AspNetCore.Mvc;
using SymptomTracker.Api.Mapping;
using SymptomTracker.Application.DTOs;
using SymptomTracker.Application.Interfaces;
using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Api.Controllers;

[ApiController]
[Route("api/v1/environment-snapshots")]
public class EnvironmentSnapshotController(IEnvironmentSnapshotRepository repo, IWeatherDataProvider provider) : Controller
{
    [HttpPost]
    [ProducesResponseType(typeof(EnvironmentSnapshotDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEnvironmentSnapshotRequest request, CancellationToken ct = default)
    {
        double pressureHpa;
        double? tempF, humidity;
        string? location;

        if (request.ManualPressureHpa.HasValue)
        {
            pressureHpa = request.ManualPressureHpa.Value;
            tempF = request.ManualTemperatureFahrenheit;
            humidity = request.ManualHumidityPercent;
            location = request.ManualLocation;
        }
        else
        {
            var reading = await provider.GetCurrentReadingAsync(request.Latitude, request.Longitude, ct);
            pressureHpa = reading.PressureHpa;
            tempF = reading.TemperatureFahrenheit;
            humidity = reading.HumidityPercent;
            location = reading.Location;
        }

        var snapshotTime = request.Timestamp?.ToUniversalTime() ?? DateTime.UtcNow;
        
        // TODO: could be changed to use multiple out params if only limited to 2 options
        var delta12Hr = await ComputeDeltaAsync(pressureHpa, snapshotTime, hours: 12, ct);
        var delta24Hr = await ComputeDeltaAsync(pressureHpa, snapshotTime, hours: 24, ct);

        var snapshot = EnvironmentSnapshot.Create(
            pressureHpa: pressureHpa,
            delta12HrHpa: delta12Hr,
            delta24HrHpa: delta24Hr,
            temperatureFahrenheit: tempF,
            humidityPercent: humidity,
            location: location,
            timestamp: snapshotTime);

        await repo.AddAsync(snapshot, ct);
        await repo.SaveChangesAsync(ct);
        
        return CreatedAtAction(nameof(GetById), new { id = snapshot.Id }, DtoMapper.ToDto(snapshot));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EnvironmentSnapshotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var snapshot = await repo.GetByIdAsync(id, ct);
        return snapshot is null ? NotFound(): Ok(DtoMapper.ToDto(snapshot));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<EnvironmentSnapshotDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWindow([FromQuery] int windowDays = 7, CancellationToken ct = default)
    {
        if (windowDays is < 1 or > 365)
            return BadRequest("windowDays must be between 1 and 365.");

        var end = DateTime.UtcNow;
        var start = end.AddDays(-windowDays);
        var snapshots = await repo.GetByWindowAsync(start, end, ct);
        return snapshots.Count == 0 ? NotFound() : Ok(snapshots.Select(DtoMapper.ToDto));
    }

    private async Task<double?> ComputeDeltaAsync(double currentPressure, DateTime snapshotTime, int hours, CancellationToken ct = default)
    {
        var horizon = snapshotTime.AddHours(-hours);
        var prior = await repo.GetMostRecentBeforeTimestampAsync(horizon, ct);
        return prior is null ? null : Math.Round(currentPressure - prior.PressureHpa, 3);
    }
}