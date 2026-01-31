using System.Text.Json;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

using SimoneCappelletti.ShortiFy.Features.Shortify;
using SimoneCappelletti.ShortiFy.Shared.Constants;

namespace SimoneCappelletti.ShortiFy.Services;

/// <summary>
/// Service for caching short URL data in Redis using the Cache-Aside pattern.
/// Registered as Singleton for optimal performance.
/// </summary>
public sealed class ShortUrlCacheService : IShortUrlCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ShortifyOptions _options;
    private readonly ILogger<ShortUrlCacheService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShortUrlCacheService"/> class.
    /// </summary>
    /// <param name="cache">The distributed cache instance.</param>
    /// <param name="options">The Shortify configuration options.</param>
    /// <param name="logger">The logger instance.</param>
    public ShortUrlCacheService(
        IDistributedCache cache,
        IOptions<ShortifyOptions> options,
        ILogger<ShortUrlCacheService> logger)
    {
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<CachedShortUrl?> GetAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(shortCode);
        var cachedValue = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (cachedValue is null)
        {
            _logger.LogDebug("Cache miss for short code: {ShortCode}", shortCode);

            return null;
        }

        _logger.LogDebug("Cache hit for short code: {ShortCode}", shortCode);

        return JsonSerializer.Deserialize<CachedShortUrl>(cachedValue);
    }

    public async Task SetAsync(string shortCode, CachedShortUrl cachedShortUrl, CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(shortCode);
        var cacheValue = JsonSerializer.Serialize(cachedShortUrl);
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheExpirationMinutes)
        };

        await _cache.SetStringAsync(cacheKey, cacheValue, cacheOptions, cancellationToken);

        _logger.LogDebug(
            "Cached short URL with key: {CacheKey}, TTL: {CacheMinutes} minutes",
            cacheKey,
            _options.CacheExpirationMinutes);
    }

    private static string BuildCacheKey(string shortCode) =>
        $"{AppConstants.CacheKeys.ShortifyPrefix}{shortCode}";
}
