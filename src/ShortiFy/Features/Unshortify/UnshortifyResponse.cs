namespace SimoneCappelletti.ShortiFy.Features.Unshortify;

/// <summary>
/// Response model containing the resolved URL details.
/// </summary>
/// <param name="OriginalUrl">The original URL that was shortened. Example: https://example.com/very/long/path</param>
/// <param name="ShortCode">The short code used to resolve the URL. Example: aBc1Xy</param>
/// <param name="ShortUrl">The complete shortened URL. Example: https://short.fy/aBc1Xy</param>
public sealed record UnshortifyResponse(string OriginalUrl, string ShortCode, string ShortUrl);
