using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;

using SimoneCappelletti.ShortiFy.Shared.Constants;

namespace SimoneCappelletti.ShortiFy.Extensions;

/// <summary>
/// Extension methods for configuring Serilog logging.
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Configures Serilog with two-stage initialization pattern.
    /// Bootstrap logger catches startup errors before full configuration loads.
    /// Includes OpenTelemetry span enrichment for TraceId/SpanId correlation
    /// and OTLP export for unified observability.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var otlpEndpoint = configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317";
        var otlpProtocol = ParseOtlpProtocol(configuration["OpenTelemetry:Protocol"]);
        var serviceName = configuration["OpenTelemetry:ServiceName"] ?? AppConstants.ApplicationName;

        services.AddSerilog((services, lc) =>
        {
            lc.ReadFrom.Configuration(configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .Enrich.WithEnvironmentName()
              .Enrich.WithMachineName()
              .Enrich.WithThreadId()
              .Enrich.WithSpan()
              .WriteTo.OpenTelemetry(options =>
              {
                  options.Endpoint = otlpEndpoint;
                  options.Protocol = otlpProtocol;
                  options.ResourceAttributes = new Dictionary<string, object>
                  {
                      ["service.name"] = serviceName
                  };
                  options.RestrictedToMinimumLevel = LogEventLevel.Information;
              });
        });

        return services;
    }

    /// <summary>
    /// Creates a bootstrap logger for capturing startup errors before full configuration.
    /// Call this at the very start of Program.cs, before WebApplication.CreateBuilder.
    /// </summary>
    public static void CreateBootstrapLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();
    }

    /// <summary>
    /// Configures Serilog request logging middleware with detailed HTTP request information.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseSerilogRequestLoggingMiddleware(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
            };
        });

        return app;
    }

    /// <summary>
    /// Parses the OTLP protocol from configuration string.
    /// Supports "Grpc" (default) and "HttpProtobuf" values.
    /// </summary>
    /// <param name="protocol">The protocol string from configuration.</param>
    /// <returns>The parsed OtlpProtocol enum value.</returns>
    private static OtlpProtocol ParseOtlpProtocol(string? protocol)
    {
        return protocol?.ToLowerInvariant() switch
        {
            "httpprotobuf" or "http" => OtlpProtocol.HttpProtobuf,
            _ => OtlpProtocol.Grpc
        };
    }
}
