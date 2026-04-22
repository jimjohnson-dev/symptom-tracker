using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using SymptomTracker.Api.Middleware;
using SymptomTracker.Application.Interfaces;
using SymptomTracker.Application.Services;
using SymptomTracker.Infrastructure;
using SymptomTracker.Infrastructure.Providers;
using SymptomTracker.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Symptom Tracker",
        Version = "v1",
        Description = "Track ongoing neurological pressure-related symptoms with barometric pressure changes."
    });
});

// Database - SQLite for small scale, minimal infra data storage
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=symptomtracker.db"));

// Repositories
builder.Services.AddScoped<ISymptomEntryRepository, SymptomEntryRepository>();
builder.Services.AddScoped<IEnvironmentSnapshotRepository, EnvironmentSnapshotRepository>();
builder.Services.AddScoped<ICorrelationResultRepository, CorrelationResultRepository>();

// Application Services
builder.Services.AddScoped<ICorrelationService, CorrelationService>();

// Weather Provider (stub)
builder.Services.AddSingleton<IWeatherDataProvider, StubWeatherDataProvider>();

var app = builder.Build();

// Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Enable Swagger for all envs, not recommended for actual prod envs
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Symptom Tracker v1"));

app.UseHttpsRedirection();
app.MapControllers();

// Migrations - auto-applying on startup for local-first SQLite. Recommend replacing with a proper CI/CD pipeline in prod envs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
