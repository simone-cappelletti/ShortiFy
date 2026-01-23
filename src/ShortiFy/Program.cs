using Serilog;

using SimoneCappelletti.ShortiFy.Extensions;

// Bootstrap logger - catches startup errors before full configuration loads
SerilogExtensions.CreateBootstrapLogger();

try
{
    Log.Information("Starting ShortiFy application");

    var builder = WebApplication.CreateBuilder(args);

    // Configure services
    builder.Services.AddSerilogLogging(builder.Configuration);
    builder.Services.AddPersistence(builder.Configuration);
    builder.Services.AddRedisCache(builder.Configuration);
    builder.Services.AddObservability(builder.Configuration);
    builder.Services.AddAppHealthChecks(builder.Configuration);

    var app = builder.Build();

    // Configure request pipeline
    app.UseSerilogRequestLoggingMiddleware();
    app.MapHealthCheckEndpoints();

    // Placeholder endpoint
    app.MapGet("/", () => "ShortiFy API is running");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
