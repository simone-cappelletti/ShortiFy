using SimoneCappelletti.ShortiFy.Features.Shortify;

namespace SimoneCappelletti.ShortiFy.Extensions;

/// <summary>
/// Extension methods for registering strongly-typed options with validation.
/// </summary>
public static class OptionsExtensions
{
    /// <summary>
    /// Registers all application options with DataAnnotations validation and startup validation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddShortifyOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ShortifyOptions>()
            .Bind(configuration.GetSection(ShortifyOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
