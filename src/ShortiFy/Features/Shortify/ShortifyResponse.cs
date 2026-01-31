namespace SimoneCappelletti.ShortiFy.Features.Shortify;

/// <summary>
/// Response model containing the shortened URL details.
/// </summary>
public sealed class ShortifyResponse
{
    /// <summary>
    /// Gets the unique short code generated for the URL.
    /// </summary>
    /// <example>aBc1Xy</example>
    public required string ShortCode { get; init; }

    /// <summary>
    /// Gets the complete shortened URL.
    /// </summary>
    /// <example>https://short.fy/aBc1Xy</example>
    public required string ShortUrl { get; init; }

    /// <summary>
    /// Gets the original URL that was shortened.
    /// </summary>
    /// <example>https://example.com/very/long/path/to/resource</example>
    public required string OriginalUrl { get; init; }
}
