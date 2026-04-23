using SymptomTracker.Application.DTOs;
using SymptomTracker.Application.Services;
using SymptomTracker.Domain.Entities;
using SymptomTracker.Domain.Enums;
using SymptomTracker.Infrastructure;
using SymptomTracker.Infrastructure.Repositories;

namespace SymptomTracker.Tests;

/// <summary>
/// Service tests use in-memory SQLite db via TestDbFactory instead of mocks
/// </summary>
public class CorrelationServiceTests : IDisposable
{
    private readonly TestDbFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    private CorrelationService BuildService(AppDbContext context) => new(
        new SymptomEntryRepository(context),
        new EnvironmentSnapshotRepository(context),
        new CorrelationResultRepository(context));
    
    // Helper factories
    private static SymptomEntry MakePatientEntry(DateTime timestamp, double severity) => SymptomEntry.Create(EntryRole.Patient, headPainLevel: severity, timestamp: timestamp);
    private static SymptomEntry MakeCaregiverEntry(DateTime timestamp, double severity) => SymptomEntry.Create(EntryRole.Caregiver, headPainLevel: severity, timestamp: timestamp);
    private static EnvironmentSnapshot MakeSnapshot(DateTime timestamp, double pressureHpa) => EnvironmentSnapshot.Create(pressureHpa: pressureHpa, timestamp: timestamp);
    
    // Tests
    [Fact]
    public async Task ReturnsInsufficientData_WhenFewerThanFivePairs()
    {
        await using var context = _factory.CreateDbContext();
        var svc = BuildService(context);
        
        var now = DateTime.UtcNow;
        
        // Insert 3 patient entries and matching snapshots; do not meet the 5-pair threshold
        for (int i = 0; i < 3; i++)
        {
            var t = now.AddHours(-i);
            context.SymptomEntries.Add(MakePatientEntry(t, 5.0));
            context.SymptomEntries.Add(MakeCaregiverEntry(t, 1010.0));
        }
        
        await context.SaveChangesAsync();
        var result = await svc.ComputeAsync(now.AddDays(-1), now, toleranceHours: 2.0);
        
        Assert.Equal(CorrelationConfidence.InsufficientData, result.Confidence);
        Assert.Null(result.PressureSeverityCorrelation);
    }

    [Fact]
    public async Task ComputesCorrectPearsonCoefficient_ForKnownDataset()
    {
        await using var context = _factory.CreateDbContext();
        var svc = BuildService(context);
        
        // Perfect negative linear relationship -> severity rises as pressure drops
        // Expected: Pearson should be exactly -1.0 for a perfect inverse linear dataset
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
            context.SymptomEntries.Add(MakePatientEntry(t, data[i].severity));
            context.EnvironmentSnapshots.Add(MakeSnapshot(t, data[i].pressure));
        }
        
        await context.SaveChangesAsync();
        var result = await svc.ComputeAsync(now.AddDays(-2), now, toleranceHours: 1.0);

        Assert.NotNull(result.PressureSeverityCorrelation);
        Assert.Equal(-1.0, result.PressureSeverityCorrelation!.Value, precision: 3);
    }

    [Fact]
    public async Task ReturnsInsufficientData_WhenNoSnapshotsInWindow()
    {
        await using var context = _factory.CreateDbContext();
        var svc = BuildService(context);

        var now = DateTime.UtcNow;
        for (int i = 0; i < 5; i++)
        {
            context.SymptomEntries.Add(MakePatientEntry(now.AddHours(-i), severity: 5.0));
        }
        
        // Save with no snapshots added
        await context.SaveChangesAsync();
        var result = await svc.ComputeAsync(now.AddDays(-1), now, toleranceHours: 2.0);
        
        Assert.Equal(CorrelationConfidence.InsufficientData, result.Confidence);
        Assert.Equal(0, result.PairedDataCount);
    }
    
    [Fact]
    public async Task ReturnsInsufficientData_WhenNoSymptomEntriesInWindow()
    {
        await using var context = _factory.CreateDbContext();
        var svc = BuildService(context);
        
        var now = DateTime.UtcNow;
        for (int i = 0; i < 5; i++)
        {
            context.EnvironmentSnapshots.Add(MakeSnapshot(now.AddHours(-i), 1010.0));
        }
        
        // No symptom entries added
        await context.SaveChangesAsync();
        var result = await svc.ComputeAsync(now.AddDays(-1), now, toleranceHours: 2.0);
        
        Assert.Equal(CorrelationConfidence.InsufficientData, result.Confidence);
        Assert.Equal(0, result.PairedDataCount);
    }

    [Fact]
    public async Task RespectsToleranceHours_ExcludesDistantSnapshots()
    {
        await using var context = _factory.CreateDbContext();
        var svc = BuildService(context);
        
        var now = DateTime.UtcNow;
        for (int i = 0; i < 5; i++)
        {
            context.SymptomEntries.Add(MakePatientEntry(now.AddHours(-i), 6.0));
        }
        
        // Place snapshots 10-14 hours in the future: every pair gap > 2hrs
        // Closest possible: entry at now vs snapshot at now+10hr -> 10hr gap)
        for (int i = 0; i < 5; i++)
        {
            context.EnvironmentSnapshots.Add(MakeSnapshot(now.AddHours(10 + i), 1010.0));
        }
        await context.SaveChangesAsync();
        var result = await svc.ComputeAsync(now.AddDays(-1), now.AddHours(15), toleranceHours: 2.0);
        
        // All potential pairs exceed 2hr tolerance, expect none to be included
        Assert.Equal(0, result.PairedDataCount);
        Assert.Equal(CorrelationConfidence.InsufficientData, result.Confidence);
    }

    [Fact]
    public async Task FilterCaregiverEntries_FromSeverityCalculation()
    {
        await using var context = _factory.CreateDbContext();
        var svc = BuildService(context);
        
        var now = DateTime.UtcNow;
        
        // Add 5 caregiver entries with high severity values & matching snapshots. Expect these entries to be filtered out
        for (int i = 0; i < 5; i++)
        {
            var t = now.AddHours(-i);
            context.SymptomEntries.Add(MakeCaregiverEntry(t, 9.0));
            context.EnvironmentSnapshots.Add(MakeSnapshot(t, 1010.0));
        }
        await context.SaveChangesAsync();
        var result = await svc.ComputeAsync(now.AddDays(-1), now, toleranceHours: 2.0);
        
        // Expect empty result set since no patient entries were added
        Assert.Equal(0, result.PairedDataCount);
        Assert.Equal(CorrelationConfidence.InsufficientData, result.Confidence);
    }

    [Theory]
    [InlineData(4, CorrelationConfidence.InsufficientData)]
    [InlineData(5, CorrelationConfidence.Low)]
    [InlineData(9, CorrelationConfidence.Low)]
    [InlineData(10, CorrelationConfidence.Medium)]
    [InlineData(29, CorrelationConfidence.Medium)]
    [InlineData(30, CorrelationConfidence.High)]
    public async Task ConfidenceLevel_ScalesWithPairCount(int pairCount, CorrelationConfidence expectedConfidence)
    {
        await using var context = _factory.CreateDbContext();
        var svc = BuildService(context);
        
        var now = DateTime.UtcNow;
        
        // Use a perfect linear dataset so the coefficient can be computed for any N >= 2
        // Pressure steps down by 1 hPa and severty steps up by 1 unit for each pair
        for (int i = 0; i < pairCount; i++)
        {
            var t = now.AddHours(-i);
            context.SymptomEntries.Add(MakePatientEntry(t, severity: 5.0 + i));
            context.EnvironmentSnapshots.Add(MakeSnapshot(t, pressureHpa: 1013.0 - i));
        }
        await context.SaveChangesAsync();

        var result = await svc.ComputeAsync(now.AddDays(-pairCount), now, toleranceHours: 1.0);
        Assert.Equal(expectedConfidence, result.Confidence);
    }

    [Fact]
    public async Task PersistsResult_ToDatabase()
    {
        await using var context = _factory.CreateDbContext();
        var svc = BuildService(context);
        
        var now = DateTime.UtcNow;
        for (int i = 0; i < 5; i++)
        {
            var t = now.AddHours(-i);
            context.SymptomEntries.Add(MakePatientEntry(t, 5.0));
            context.EnvironmentSnapshots.Add(MakeSnapshot(t, 1010.0));
        }
        await context.SaveChangesAsync();
        var result = await svc.ComputeAsync(now.AddDays(-1), now, toleranceHours: 2.0);
        
        // Verify result was saved to the db
        var persisted = await context.CorrelationResults.FindAsync(result.Id);
        Assert.NotNull(persisted);
        Assert.Equal(result.Id, persisted!.Id);
    }
}