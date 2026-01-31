using System.ComponentModel.DataAnnotations;

namespace SimoneCappelletti.ShortiFy.Features.Shortify;

/// <summary>
/// Request model for creating a shortened URL.
/// </summary>
public sealed class ShortifyRequest
{
    /// <summary>
    /// Gets or sets the original URL to shorten.
    /// Must be a valid HTTP or HTTPS URL.
    /// </summary>
    /// <example>https://example.com/very/long/path/to/resource</example>
    [Required(ErrorMessage = "OriginalUrl is required.")]
    [MaxLength(2048, ErrorMessage = "OriginalUrl must not exceed 2048 characters.")]
    public required string OriginalUrl { get; init; }
}
