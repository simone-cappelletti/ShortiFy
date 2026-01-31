using SimoneCappelletti.ShortiFy.Features.Shortify;
using SimoneCappelletti.ShortiFy.Features.Unshortify;
using SimoneCappelletti.ShortiFy.Services;

namespace SimoneCappelletti.ShortiFy.Extensions;

/// <summary>
/// Extension methods for mapping API endpoints.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Maps the Shortify feature endpoints.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication MapShortifyEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api").WithTags("Shortify");

        group.MapPost("/shortify", ShortifyEndpoint.HandleAsync)
            .WithName("Shortify")
            .WithSummary("Create a shortened URL")
            .WithDescription("Creates a shortened URL for the provided original URL. Returns existing short URL if already shortened (idempotent).")
            .Produces<ShortifyResponse>(StatusCodes.Status201Created)
            .Produces<ShortifyResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/unshortify/{shortCode}", UnshortifyEndpoint.HandleAsync)
            .WithName("Unshortify")
            .WithSummary("Resolve a shortened URL")
            .WithDescription("Resolves a short code back to the original URL. Uses Cache-Aside pattern for fast lookups.")
            .Produces<UnshortifyResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    /// <summary>
    /// Registers Shortify-related services in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddShortifyServices(this IServiceCollection services)
    {
        services.AddSingleton<ShortCodeService>();

        return services;
    }
}
