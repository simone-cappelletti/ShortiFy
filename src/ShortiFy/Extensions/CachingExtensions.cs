using SimoneCappelletti.ShortiFy.Shared.Constants;

using StackExchange.Redis;

namespace SimoneCappelletti.ShortiFy.Extensions;

/// <summary>
/// Extension methods for configuring Redis distributed caching.
/// </summary>
public static class CachingExtensions
{
    /// <summary>
    /// Configures Redis distributed cache and registers ConnectionMultiplexer for instrumentation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when Redis connection string is not configured.</exception>
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString(AppConstants.ConnectionStrings.Redis)
            ?? throw new InvalidOperationException($"Connection string '{AppConstants.ConnectionStrings.Redis}' not found.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = $"{AppConstants.ApplicationName}:";
        });

        // Register Redis ConnectionMultiplexer for OpenTelemetry instrumentation
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        return services;
    }
}
