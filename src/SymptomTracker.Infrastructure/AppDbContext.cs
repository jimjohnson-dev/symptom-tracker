using Microsoft.EntityFrameworkCore;
using SymptomTracker.Domain.Entities;

namespace SymptomTracker.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<CorrelationResult> CorrelationResults => Set<CorrelationResult>();
    public DbSet<EnvironmentSnapshot> EnvironmentSnapshots => Set<EnvironmentSnapshot>();
    public DbSet<SymptomEntry> SymptomEntries => Set<SymptomEntry>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureSymptomEntry(modelBuilder);
        ConfigureEnvironmentSnapshot(modelBuilder);
        ConfigureCorrelationResult(modelBuilder);
    }

    private static void ConfigureSymptomEntry(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SymptomEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Timestamp).IsRequired();
            e.Property(x => x.Role).IsRequired().HasConversion<string>();
            
            // Computed value, not persisted - ignore
            e.Ignore(x => x.AverageSeverity);
            
            // Avoid full table scans -> query pattern relies on windowStart and windowEnd times
            e.HasIndex(x => x.Timestamp).HasDatabaseName("IX_SymptomEntries_Timestamp");
        });
    }

    private static void ConfigureEnvironmentSnapshot(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EnvironmentSnapshot>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Timestamp).IsRequired();
            e.Property(x => x.PressureHpa).IsRequired();

            e.HasIndex(x => x.Timestamp).HasDatabaseName("IX_EnvironmentSnapshots_Timestamp");
        });
    }

    private static void ConfigureCorrelationResult(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CorrelationResult>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ComputedAt).IsRequired();
            e.Property(x => x.Confidence).IsRequired().HasConversion<string>();
            
            // Use ComputedAt instead of by Id for future filtering on list of recent results
            e.HasIndex(x => x.ComputedAt).HasDatabaseName("IX_CorrelationResults_ComputedAt");
        });
    }
}