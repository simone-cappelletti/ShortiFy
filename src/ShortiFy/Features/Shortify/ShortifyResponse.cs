namespace SimoneCappelletti.ShortiFy.Features.Shortify;

/// <summary>
/// Response model containing the shortened URL details.
/// </summary>
/// <param name="ShortCode">The unique short code generated for the URL. Example: aBc1Xy</param>
/// <param name="ShortUrl">The complete shortened URL. Example: https://short.fy/aBc1Xy</param>
/// <param name="OriginalUrl">The original URL that was shortened. Example: https://example.com/very/long/path</param>
public sealed record ShortifyResponse(string ShortCode, string ShortUrl, string OriginalUrl);
