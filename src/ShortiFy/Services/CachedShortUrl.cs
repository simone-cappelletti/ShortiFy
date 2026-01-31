namespace SimoneCappelletti.ShortiFy.Services;

/// <summary>
/// Record for caching short URL data in Redis.
/// </summary>
/// <param name="OriginalUrl">The original URL that was shortened.</param>
/// <param name="ShortenUrl">The complete shortened URL.</param>
public sealed record CachedShortUrl(string OriginalUrl, string ShortenUrl);
