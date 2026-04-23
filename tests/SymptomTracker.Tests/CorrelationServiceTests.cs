using SymptomTracker.Application.DTOs;
using SymptomTracker.Application.Services;
using SymptomTracker.Domain.Entities;
using SymptomTracker.Domain.Enums;
using SymptomTracker.Infrastructure;
using SymptomTracker.Infrastructure.Repositories;

namespace SymptomTracker.Tests;

public class CorrelationServiceTests : IDisposable
{
    // tests use in-memory SQLite db instead of mocks
    private readonly TestDbFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    private CorrelationService BuildService(AppDbContext ctx) => new(
        new SymptomEntryRepository(ctx),
        new EnvironmentSnapshotRepository(ctx),
        new CorrelationResultRepository(ctx));
    
    private static SymptomEntry MakePatientEntry(DateTime timestamp, double severity) => SymptomEntry.Create(EntryRole.Patient, headPainLevel: severity, timestamp: timestamp);
    private static SymptomEntry MakeCaregiverEntry(DateTime timestamp, double severity) => SymptomEntry.Create(EntryRole.Caregiver, headPainLevel: severity, timestamp: timestamp);
    private static EnvironmentSnapshot MakeSnapshot(DateTime timestamp, double pressureHpa) => EnvironmentSnapshot.Create(pressureHpa: pressureHpa, timestamp: timestamp);
    
    [Fact]
    public async Task BelowMinThresholdReturnsInsufficientData()
    {
        await using var ctx = _factory.CreateDbContext();
        var svc = BuildService(ctx);
        var now = DateTime.UtcNow;
        
        // add less than the threshold of patient entries and matching snapshots
        for (int i = 0; i < 3; i++)
        {
            var t = now.AddHours(-i);
            ctx.SymptomEntries.Add(MakePatientEntry(t, 5.0));
            ctx.SymptomEntries.Add(MakeCaregiverEntry(t, 1010.0));
        }
        
        await ctx.SaveChangesAsync();
        var result = await svc.ComputeCorrelationAsync(now.AddDays(-1), now, toleranceHours: 2.0);
        
        Assert.Equal(CorrelationConfidence.InsufficientData, result.Confidence);
        Assert.Null(result.PressureSeverityCorrelation);
    }

    [Fact]
    public async Task PearsonReturnsCorrectValForPerfectInverseData()
    {
        await using var ctx = _factory.CreateDbContext();
        var svc = BuildService(ctx);
        
        // severity usually rises when pressure drops, should be -1 for a perfect inverse
        var now = DateTime.UtcNow;
        var data = new (double pressure, double severity)[]
        {
            (1020.0, 2.0),
            (1015.0, 4.0),
            (1010.0, 6.0),
            (1005.0, 8.0),
            (1000.0, 10.0)
        };

        for (int i = 0; i < data.Length; i++)
        {
            var t = now.AddHours(-i * 2);
            ctx.SymptomEntries.Add(MakePatientEntry(t, data[i].severity));
            ctx.EnvironmentSnapshots.Add(MakeSnapshot(t, data[i].pressure));
        }
        
        await ctx.SaveChangesAsync();
        var result = await svc.ComputeCorrelationAsync(now.AddDays(-2), now, toleranceHours: 1.0);

        Assert.NotNull(result.PressureSeverityCorrelation);
        Assert.Equal(-1.0, result.PressureSeverityCorrelation!.Value, precision: 3);
    }

    [Fact]
    public async Task WhenNoSnapshotsInWindowReturnsCorrectVal()
    {
        await using var ctx = _factory.CreateDbContext();
        var svc = BuildService(ctx);
        var now = DateTime.UtcNow;
        
        for (int i = 0; i < 5; i++)
            ctx.SymptomEntries.Add(MakePatientEntry(now.AddHours(-i), severity: 5.0));
        
        await ctx.SaveChangesAsync();
        var result = await svc.ComputeCorrelationAsync(now.AddDays(-1), now, toleranceHours: 2.0);
        
        Assert.Equal(CorrelationConfidence.InsufficientData, result.Confidence);
        Assert.Equal(0, result.PairedDataCount);
    }
    
    [Fact]
    public async Task WhenNoSymptomEntriesInWindowReturnsCorrectVal()
    {
        await using var ctx = _factory.CreateDbContext();
        var svc = BuildService(ctx);
        var now = DateTime.UtcNow;
        
        for (int i = 0; i < 5; i++)
            ctx.EnvironmentSnapshots.Add(MakeSnapshot(now.AddHours(-i), 1010.0));
        
        await ctx.SaveChangesAsync();
        var result = await svc.ComputeCorrelationAsync(now.AddDays(-1), now, toleranceHours: 2.0);
        
        Assert.Equal(CorrelationConfidence.InsufficientData, result.Confidence);
        Assert.Equal(0, result.PairedDataCount);
    }

    [Fact]
    public async Task ExcludesDistantSnapshots()
    {
        await using var ctx = _factory.CreateDbContext();
        var svc = BuildService(ctx);
        var now = DateTime.UtcNow;
        for (int i = 0; i < 5; i++)
            ctx.SymptomEntries.Add(MakePatientEntry(now.AddHours(-i), 6.0));
        
        // make snapshots outside the normal tolerance window
        for (int i = 0; i < 5; i++)
            ctx.EnvironmentSnapshots.Add(MakeSnapshot(now.AddHours(10 + i), 1010.0));

        await ctx.SaveChangesAsync();
        var result = await svc.ComputeCorrelationAsync(now.AddDays(-1), now.AddHours(15), toleranceHours: 2.0);
        
        Assert.Equal(0, result.PairedDataCount);
        Assert.Equal(CorrelationConfidence.InsufficientData, result.Confidence);
    }

    [Fact]
    public async Task SeverityCalculationShouldNotIncludeCaregiverEntries()
    {
        await using var ctx = _factory.CreateDbContext();
        var svc = BuildService(ctx);
        var now = DateTime.UtcNow;
        
        // caregiver entries are observed and would add noise
        for (int i = 0; i < 5; i++)
        {
            var t = now.AddHours(-i);
            ctx.SymptomEntries.Add(MakeCaregiverEntry(t, 9.0));
            ctx.EnvironmentSnapshots.Add(MakeSnapshot(t, 1010.0));
        }
        await ctx.SaveChangesAsync();
        var result = await svc.ComputeCorrelationAsync(now.AddDays(-1), now, toleranceHours: 2.0);
        
        Assert.Equal(0, result.PairedDataCount);
        Assert.Equal(CorrelationConfidence.InsufficientData, result.Confidence);
    }

    [Fact]
    public async Task ResultsPersistToDatabase()
    {
        await using var ctx = _factory.CreateDbContext();
        var svc = BuildService(ctx);
        
        var now = DateTime.UtcNow;
        
        // varying severity/pressure - Pearson can return NaN when all vals are the same, breaks SQLite
        for (int i = 0; i < 5; i++)
        {
            var t = now.AddHours(-i);
            ctx.SymptomEntries.Add(MakePatientEntry(t, 5.0 + i));
            ctx.EnvironmentSnapshots.Add(MakeSnapshot(t, 1013.0 - i));
        }
        await ctx.SaveChangesAsync();
        var result = await svc.ComputeCorrelationAsync(now.AddDays(-1), now, toleranceHours: 2.0);
        
        var persisted = await ctx.CorrelationResults.FindAsync(result.Id);
        Assert.NotNull(persisted);
        Assert.Equal(result.Id, persisted!.Id);
    }

    [Fact]
    public async Task CoefficientIsNullWhenAllSeveritiesIdentical()
    {
        await using var ctx = _factory.CreateDbContext();
        var svc = BuildService(ctx);
        var now = DateTime.UtcNow;
        for (int i = 0; i < 5; i++)
        {
            var t = now.AddHours(-i);
            ctx.SymptomEntries.Add(MakePatientEntry(t, 5.0));
            ctx.EnvironmentSnapshots.Add(MakeSnapshot(t, 1010.0 - i));
        }
        
        await ctx.SaveChangesAsync();
        var result = await svc.ComputeCorrelationAsync(now.AddDays(-1), now, toleranceHours: 2);
        Assert.Null(result.PressureSeverityCorrelation);
    }
}