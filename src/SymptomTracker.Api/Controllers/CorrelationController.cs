using Microsoft.AspNetCore.Mvc;
using SymptomTracker.Api.Mapping;
using SymptomTracker.Application.DTOs;
using SymptomTracker.Application.Interfaces;

namespace SymptomTracker.Api.Controllers;

[ApiController]
[Route("api/v1/correlation")]
public class CorrelationController(ICorrelationService service, ICorrelationResultRepository repo) : Controller
{
    [HttpPost]
    [ProducesResponseType(typeof(CorrelationResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Compute([FromBody] CorrelationRequestDto request, CancellationToken cancellationToken)
    {
        var end = DateTime.UtcNow;
        var start = end.AddDays(-request.WindowDays);
        
        var result = await service.ComputeAsync(start, end, request.ToleranceHours, cancellationToken);
        var dto = DtoMapper.ToDto(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, dto);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CorrelationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await repo.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound(): Ok(DtoMapper.ToDto(result));
    }
}