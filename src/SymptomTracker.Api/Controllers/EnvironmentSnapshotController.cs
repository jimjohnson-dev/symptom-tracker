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
    public async Task<IActionResult> Create([FromBody] CreateEnvironmentSnapshotRequest request, CancellationToken cancellationToken)
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
            var reading = await provider.GetCurrentReadingAsync(request.Latitude, request.Longitude, cancellationToken);
            pressureHpa = reading.PressureHpa;
            tempF = reading.TemperatureFahrenheit;
            humidity = reading.HumidityPercent;
            location = reading.Location;
        }

        var snapshotTime = request.Timestamp?.ToUniversalTime() ?? DateTime.UtcNow;
        
        // Compute pressure deltas using the most recent snapshots before this timestamp to keep the delta relative to the data capture and repeatable
        var delta12Hr = await ComputeDeltaAsync(pressureHpa, snapshotTime, hours: 12, cancellationToken);
        var delta24Hr = await ComputeDeltaAsync(pressureHpa, snapshotTime, hours: 24, cancellationToken);

        var snapshot = EnvironmentSnapshot.Create(
            pressureHpa: pressureHpa,
            delta12HrHpa: delta12Hr,
            delta24HrHpa: delta24Hr,
            temperatureFahrenheit: tempF,
            humidityPercent: humidity,
            location: location,
            timestamp: snapshotTime);

        await repo.AddAsync(snapshot, cancellationToken);
        await repo.SaveChangesAsync(cancellationToken);
        
        var dto = DtoMapper.ToDto(snapshot);
        return CreatedAtAction(nameof(GetById), new { id = snapshot.Id }, dto);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EnvironmentSnapshotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var snapshot = await repo.GetByIdAsync(id, cancellationToken);
        return snapshot is null ? NotFound(): Ok(DtoMapper.ToDto(snapshot));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<EnvironmentSnapshotDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWindow([FromQuery] int windowDays = 7, CancellationToken cancellationToken = default)
    {
        if (windowDays is < 1 or > 365)
            return BadRequest("windowDays must be between 1 and 365.");

        var end = DateTime.UtcNow;
        var start = end.AddDays(-windowDays);
        var snapshots = await repo.GetByWindowAsync(start, end, cancellationToken);
        return Ok(snapshots.Select(DtoMapper.ToDto));
    }

    /// <summary>
    /// Finds the most recent snapshot before given snapshotTime - hoursBack and returns pressure difference,
    /// or null if no prior snapshot exists in given time horizon
    /// </summary>
    private async Task<double?> ComputeDeltaAsync(double currentPressure, DateTime snapshotTime, int hours, CancellationToken cancellationToken)
    {
        var horizon = snapshotTime.AddHours(-hours);
        var prior = await repo.GetMostRecentBeforeAsync(horizon, cancellationToken);
        return prior is null ? null : Math.Round(currentPressure - prior.PressureHpa, 3);
    }
}