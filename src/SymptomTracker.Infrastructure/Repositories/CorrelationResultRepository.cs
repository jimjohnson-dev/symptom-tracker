using Microsoft.EntityFrameworkCore;
using SymptomTracker.Application.Interfaces;
using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Infrastructure.Repositories;

public class CorrelationResultRepository(AppDbContext context) : ICorrelationResultRepository
{
    public Task<CorrelationResult?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) 
        => context.CorrelationResults.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task AddAsync(CorrelationResult result, CancellationToken cancellationToken = default)
        =>  await context.CorrelationResults.AddAsync(result, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}