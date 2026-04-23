using SymptomTracker.Domain.Entities;
using SymptomTracker.Domain.Enums;

namespace SymptomTracker.Tests;

public class SymptomEntryTests
{
    [Fact]
    public void OverallSeverityReturnsNullWhenNoValuesAreSet()
    {
        var entry = SymptomEntry.Create(
            role: EntryRole.Patient,
            notes: "Only add a note.");
        
        Assert.Null(entry.OverallSeverity);
    }

    [Fact]
    public void OverallSeverityAveragesOnlyNonNullValues()
    {
        // Set 3/5 values, expect average to ignore other fields
        var entry = SymptomEntry.Create(
            role: EntryRole.Patient,
            headPainLevel: 8.0,
            eyePressure: 6.0,
            fatigue: 4.0);
        
        // Expected: (8 + 6 + 4) / 3 = 6.0
        Assert.NotNull(entry.OverallSeverity);
        Assert.Equal(6.0, entry.OverallSeverity!.Value, precision: 5);
    }

    [Fact]
    public void CreateDefaultsToUtcNowTimestamp()
    {
        var before = DateTime.UtcNow;
        var entry = SymptomEntry.Create(role: EntryRole.Patient);
        var after = DateTime.UtcNow;
        
        Assert.InRange(entry.Timestamp, before, after);
        Assert.Equal(DateTimeKind.Utc, entry.Timestamp.Kind);
    }

    [Fact]
    public void CreateConvertsProvidedTimestampToUtc()
    {
        var localTime = new DateTime(2025, 3, 14, 15, 09, 0, DateTimeKind.Local);
        var entry = SymptomEntry.Create(role: EntryRole.Patient, timestamp: localTime);
        
        Assert.Equal(DateTimeKind.Utc, entry.Timestamp.Kind);
        Assert.Equal(localTime.ToUniversalTime(), entry.Timestamp);
    }
}