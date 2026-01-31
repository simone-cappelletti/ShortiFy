namespace SimoneCappelletti.ShortiFy.Features.Unshortify;

/// <summary>
/// Response model containing the resolved URL details.
/// </summary>
public sealed class UnshortifyResponse
{
    /// <summary>
    /// Gets the original URL that was shortened.
    /// </summary>
    /// <example>https://example.com/very/long/path/to/resource</example>
    public required string OriginalUrl { get; init; }

    /// <summary>
    /// Gets the short code used to resolve the URL.
    /// </summary>
    /// <example>aBc1Xy</example>
    public required string ShortCode { get; init; }

    /// <summary>
    /// Gets the complete shortened URL.
    /// </summary>
    /// <example>https://short.fy/aBc1Xy</example>
    public required string ShortUrl { get; init; }
}
