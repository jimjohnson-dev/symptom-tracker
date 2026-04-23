using Microsoft.AspNetCore.Mvc;
using SymptomTracker.Api.Mapping;
using SymptomTracker.Application.DTOs;
using SymptomTracker.Application.Interfaces;
using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Api.Controllers;

[ApiController]
[Route("api/v1/symptom-entries")]
public class SymptomEntryController(ISymptomEntryRepository repo) : Controller
{
    [HttpPost]
    [ProducesResponseType(typeof(SymptomEntryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSymptomEntryRequest request, CancellationToken ct = default)
    {
        var entry = SymptomEntry.Create(
            role: request.Role,
            timestamp: request.Timestamp,
            headPainLevel: request.HeadPainLevel,
            eyePressure: request.EyePressure,
            visionClarity: request.VisionClarity,
            fatigue: request.Fatigue,
            nausea: request.Nausea,
            aphasia: request.Aphasia,
            confusion: request.Confusion,
            notes: request.Notes);
        
        await repo.AddAsync(entry, ct);
        await repo.SaveChangesAsync(ct);
        
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, DtoMapper.ToDto(entry));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SymptomEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var entry = await repo.GetByIdAsync(id, ct);
        // return entry is null ? NotFound() : Ok(DtoMapper.ToDto(entry));
        if (entry is null) return NotFound("Symptom entry not found");
        return Ok(DtoMapper.ToDto(entry));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<SymptomEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWindow([FromQuery] int windowDays = 7, CancellationToken ct = default)
    {
        if (windowDays is < 1 or > 365)
            return BadRequest("windowDays must be between 1 and 365.");

        var end = DateTime.UtcNow;
        var start = end.AddDays(-windowDays);
        var entries = await repo.GetByWindowAsync(start, end, ct);
        return Ok(entries.Select(DtoMapper.ToDto));
    }
}