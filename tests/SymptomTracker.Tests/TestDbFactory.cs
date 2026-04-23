using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SymptomTracker.Infrastructure;

namespace SymptomTracker.Tests;

/// <summary>
/// Creates an in-memory SQLite database for each test class
/// </summary>
public sealed class TestDbFactory : IDisposable
{
    private readonly SqliteConnection _connection;
    
    public TestDbFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
    
    public void Dispose() => _connection.Dispose();
}