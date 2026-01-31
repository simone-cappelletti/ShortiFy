using System.ComponentModel.DataAnnotations;

namespace SimoneCappelletti.ShortiFy.Features.Shortify;

/// <summary>
/// Configuration options for the Shortify feature.
/// Validated at startup using DataAnnotations.
/// </summary>
public sealed class ShortifyOptions
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Shortify";

    /// <summary>
    /// Gets or sets the base URL for generating shortened URLs.
    /// </summary>
    /// <example>https://short.fy/</example>
    [Required(ErrorMessage = "BaseUrl is required.")]
    public required string BaseUrl { get; init; }

    /// <summary>
    /// Gets or sets the cache expiration time in minutes.
    /// </summary>
    /// <example>60</example>
    [Range(1, 1440, ErrorMessage = "CacheExpirationMinutes must be between 1 and 1440.")]
    public int CacheExpirationMinutes { get; init; } = 60;
}
