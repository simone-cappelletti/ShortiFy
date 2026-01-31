using Serilog;

using SimoneCappelletti.ShortiFy.Extensions;

SerilogExtensions.CreateBootstrapLogger();

try
{
    Log.Information("Starting ShortiFy application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilogLogging(builder.Configuration);
    builder.Services.AddShortifyOptions(builder.Configuration);
    builder.Services.AddPersistence(builder.Configuration);
    builder.Services.AddRedisCache(builder.Configuration);
    builder.Services.AddObservability(builder.Configuration);
    builder.Services.AddAppHealthChecks(builder.Configuration);
    builder.Services.AddShortifyServices();

    var app = builder.Build();

    app.UseSerilogRequestLoggingMiddleware();
    app.MapHealthCheckEndpoints();
    app.MapShortifyEndpoints();

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
