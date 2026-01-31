namespace SimoneCappelletti.ShortiFy.Services;

/// <summary>
/// Service interface for caching short URL data in Redis.
/// </summary>
public interface IShortUrlCacheService
{
    /// <summary>
    /// Gets a cached short URL by its short code.
    /// </summary>
    /// <param name="shortCode">The short code to look up.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>The cached short URL data, or null if not found.</returns>
    Task<CachedShortUrl?> GetAsync(string shortCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a short URL in the cache with the configured TTL.
    /// </summary>
    /// <param name="shortCode">The short code as the cache key.</param>
    /// <param name="cachedShortUrl">The short URL data to cache.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    Task SetAsync(string shortCode, CachedShortUrl cachedShortUrl, CancellationToken cancellationToken = default);
}
