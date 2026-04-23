using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using SymptomTracker.Api.Middleware;
using SymptomTracker.Application.Interfaces;
using SymptomTracker.Application.Services;
using SymptomTracker.Infrastructure;
using SymptomTracker.Infrastructure.Providers;
using SymptomTracker.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// add Controllers
builder.Services.AddControllers();

// add Swagger
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

// add Database - SQLite fine for this project, switching to a different db should be trivial with this
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=symptomtracker.db"));

// add Repositories
builder.Services.AddScoped<ICorrelationResultRepository, CorrelationResultRepository>();
builder.Services.AddScoped<IEnvironmentSnapshotRepository, EnvironmentSnapshotRepository>();
builder.Services.AddScoped<ISymptomEntryRepository, SymptomEntryRepository>();

// add Services
builder.Services.AddScoped<ICorrelationService, CorrelationService>();

// add Weather Provider (stub)
builder.Services.AddSingleton<IWeatherDataProvider, StubWeatherDataProvider>();

var app = builder.Build();

// add Middleware - slightly overkill for this app but low-cost to start with this
app.UseMiddleware<GlobalExceptionMiddleware>();

// add Swagger for all envs, not recommended for actual prod envs
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Symptom Tracker v1"));

app.UseHttpsRedirection();
app.MapControllers();

// handle EF Migrations - auto-applying on startup ok for this version, full ci/cd pipeline would be overkill here
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
