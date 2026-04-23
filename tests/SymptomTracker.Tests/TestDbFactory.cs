using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SymptomTracker.Infrastructure;

namespace SymptomTracker.Tests;

// use real data over mocks to catch ordering and boundary conditions
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
        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }
    
    public void Dispose() => _connection.Dispose();
}