using Microsoft.AspNetCore.Mvc;
using SymptomTracker.Api.Mapping;
using SymptomTracker.Application.DTOs;
using SymptomTracker.Application.Interfaces;
using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Api.Controllers;

[ApiController]
[Route("api/v1/symptom-entry")]
public class SymptomEntryController(ISymptomEntryRepository repository) : Controller
{
    [HttpPost]
    [ProducesResponseType(typeof(SymptomEntryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSymptomEntryRequest request, CancellationToken cancellationToken)
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
        
        await repository.AddAsync(entry, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        
        var dto = DtoMapper.ToDto(entry);
        return CreatedAtAction(nameof(GetById), new { id = entry.Id }, dto);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SymptomEntryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var entry = await repository.GetByIdAsync(id, cancellationToken);
        return entry is null ? NotFound() : Ok(DtoMapper.ToDto(entry));
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<SymptomEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWindow([FromQuery] int windowDays = 7, CancellationToken cancellationToken = default)
    {
        if (windowDays is < 1 or > 365)
            return BadRequest("windowDays must be between 1 and 365.");

        var end = DateTime.UtcNow;
        var start = end.AddDays(-windowDays);
        var entries = await repository.GetByWindowAsync(start, end, cancellationToken);
        return Ok(entries.Select(DtoMapper.ToDto));
    }
}