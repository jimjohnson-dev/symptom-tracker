using SymptomTracker.Domain.Entities;
using SymptomTracker.Domain.Enums;

namespace SymptomTracker.Tests;

public class SymptomEntryTests
{
    [Fact]
    public void OverallSeverity_ReturnsNull_WhenNoValuesAreSet()
    {
        var entry = SymptomEntry.Create(
            role: EntryRole.Patient,
            notes: "Only add a note.");
        
        Assert.Null(entry.OverallSeverity);
    }

    [Fact]
    public void OverallSeverity_AveragesOnlyNonNullValues()
    {
        // Set 3/5 values. Average must ignore the two null fields.
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
    public void Create_SetsTimestamp_ToUtcNow_WhenNotProvided()
    {
        var before = DateTime.UtcNow;
        var entry = SymptomEntry.Create(role: EntryRole.Patient);
        var after = DateTime.UtcNow;
        
        Assert.InRange(entry.Timestamp, before, after);
        Assert.Equal(DateTimeKind.Utc, entry.Timestamp.Kind);
    }

    [Fact]
    public void Create_UsesProvidedTimestamp_ConvertedToUtc()
    {
        var localTime = new DateTime(2025, 3, 14, 15, 09, 0, DateTimeKind.Local);
        var entry = SymptomEntry.Create(role: EntryRole.Patient, timestamp: localTime);
        
        Assert.Equal(DateTimeKind.Utc, entry.Timestamp.Kind);
        Assert.Equal(localTime.ToUniversalTime(), entry.Timestamp);
    }
}