using Microsoft.AspNetCore.Mvc;
using SymptomTracker.Api.Mapping;
using SymptomTracker.Application.DTOs;
using SymptomTracker.Application.Interfaces;

namespace SymptomTracker.Api.Controllers;

[ApiController]
[Route("api/v1/correlation")]
public class CorrelationController(ICorrelationService service, ICorrelationResultRepository repo) : Controller
{
    // calculates the Pearson correlation over a time window and stores the result
    [HttpPost]
    [ProducesResponseType(typeof(CorrelationResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Compute([FromBody] CorrelationRequestDto request, CancellationToken ct = default)
    {
        var end = DateTime.UtcNow;
        var start = end.AddDays(-request.WindowDays);
        
        var result = await service.ComputeCorrelationAsync(start, end, request.ToleranceHours, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, DtoMapper.ToDto(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CorrelationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await repo.GetByIdAsync(id, ct);
        return result is null ? NotFound(): Ok(DtoMapper.ToDto(result));
    }
}