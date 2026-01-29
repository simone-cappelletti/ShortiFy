using Microsoft.EntityFrameworkCore;

using Serilog;

using SimoneCappelletti.ShortiFy.Extensions;
using SimoneCappelletti.ShortiFy.Infrastructure.Persistence;

SerilogExtensions.CreateBootstrapLogger();

try
{
    Log.Information("Starting ShortiFy application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilogLogging(builder.Configuration);
    builder.Services.AddPersistence(builder.Configuration);
    builder.Services.AddRedisCache(builder.Configuration);
    builder.Services.AddObservability(builder.Configuration);
    builder.Services.AddAppHealthChecks(builder.Configuration);

    var app = builder.Build();

    await ApplyMigrationsAsync(app);

    app.UseSerilogRequestLoggingMiddleware();
    app.MapHealthCheckEndpoints();

    // Placeholder endpoint
    app.MapGet("/", () => "Hello ShortiFy!");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

/// <summary>
/// Applies pending database migrations on application startup.
/// </summary>
/// <param name="app">The web application instance.</param>
static async Task ApplyMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ShortiFyDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Checking for pending database migrations...");

        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        var migrations = pendingMigrations.ToList();

        if (migrations.Any())
        {
            logger.LogInformation("Applying {MigrationCount} pending migration(s): {Migrations}",
                migrations.Count, string.Join(", ", migrations));

            await dbContext.Database.MigrateAsync();

            logger.LogInformation("Database migrations applied successfully");
        }
        else
        {
            logger.LogInformation("Database is up to date, no migrations to apply");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations");
        throw;
    }
}
