using Microsoft.EntityFrameworkCore;
using SymptomTracker.Application.Interfaces;
using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Infrastructure.Repositories;

public class CorrelationResultRepository(AppDbContext ctx) : ICorrelationResultRepository
{
    public Task<CorrelationResult?> GetByIdAsync(Guid id, CancellationToken ct = default) 
        => ctx.CorrelationResults.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddAsync(CorrelationResult result, CancellationToken ct = default)
        =>  await ctx.CorrelationResults.AddAsync(result, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => ctx.SaveChangesAsync(ct);
}