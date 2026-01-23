using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

using SimoneCappelletti.ShortiFy.Infrastructure.Persistence;

namespace SimoneCappelletti.ShortiFy.Extensions;

/// <summary>
/// Extension methods for configuring health checks.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds health checks for SQL Server, Redis, and EF Core DbContext.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required connection strings are not configured.</exception>
    public static IServiceCollection AddAppHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string 'Redis' not found.");

        services.AddHealthChecks()
            .AddSqlServer(
                connectionString,
                name: "sqlserver",
                tags: new[] { "db", "sql", "ready" })
            .AddRedis(
                redisConnectionString,
                name: "redis",
                tags: new[] { "cache", "redis", "ready" })
            .AddDbContextCheck<ShortiFyDbContext>(
                name: "efcore",
                tags: new[] { "db", "efcore", "ready" });

        return services;
    }

    /// <summary>
    /// Maps health check endpoints for liveness and readiness probes.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteHealthCheckResponse
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteHealthCheckResponse
        });

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

        return app;
    }

    private static Task WriteHealthCheckResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var options = new JsonWriterOptions { Indented = true };

        using var memoryStream = new MemoryStream();
        using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WriteString("status", report.Status.ToString());
            jsonWriter.WriteString("totalDuration", report.TotalDuration.ToString());
            jsonWriter.WriteStartObject("checks");

            foreach (var entry in report.Entries)
            {
                jsonWriter.WriteStartObject(entry.Key);
                jsonWriter.WriteString("status", entry.Value.Status.ToString());
                jsonWriter.WriteString("duration", entry.Value.Duration.ToString());

                if (entry.Value.Description is not null)
                {
                    jsonWriter.WriteString("description", entry.Value.Description);
                }

                if (entry.Value.Exception is not null)
                {
                    jsonWriter.WriteString("exception", entry.Value.Exception.Message);
                }

                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();
        }

        return context.Response.WriteAsync(Encoding.UTF8.GetString(memoryStream.ToArray()));
    }
}
