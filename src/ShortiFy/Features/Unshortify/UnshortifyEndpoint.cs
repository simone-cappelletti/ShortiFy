using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

using SimoneCappelletti.ShortiFy.Features.Shortify;
using SimoneCappelletti.ShortiFy.Infrastructure.Persistence;
using SimoneCappelletti.ShortiFy.Shared.Constants;

namespace SimoneCappelletti.ShortiFy.Features.Unshortify;

/// <summary>
/// Endpoint for resolving shortened URLs back to their original form.
/// </summary>
public static class UnshortifyEndpoint
{
    /// <summary>
    /// Handles the GET /api/unshortify/{shortCode} request to resolve a shortened URL.
    /// Implements Cache-Aside pattern: checks Redis first, falls back to SQL Server on cache miss.
    /// </summary>
    /// <param name="shortCode">The short code to resolve.</param>
    /// <param name="dbContext">The database context for persistence.</param>
    /// <param name="cache">The distributed cache for Redis operations.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="logger">The logger for structured logging.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>
    /// 200 OK with UnshortifyResponse if found.
    /// 404 Not Found with Problem Details if short code doesn't exist.
    /// </returns>
    public static async Task<IResult> HandleAsync(
        string shortCode,
        ShortiFyDbContext dbContext,
        IDistributedCache cache,
        IConfiguration configuration,
        ILogger<UnshortifyResponse> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing unshortify request for short code: {ShortCode}", shortCode);

        var cacheKey = $"{AppConstants.CacheKeys.ShortifyPrefix}{shortCode}";

        // Step 1: Check Redis cache first
        var cachedValue = await cache.GetStringAsync(cacheKey, cancellationToken);

        if (cachedValue is not null)
        {
            logger.LogDebug("Cache hit for short code: {ShortCode}", shortCode);

            var cachedUrl = JsonSerializer.Deserialize<CachedShortUrl>(cachedValue);

            if (cachedUrl is not null)
            {
                return Results.Ok(new UnshortifyResponse
                {
                    OriginalUrl = cachedUrl.OriginalUrl,
                    ShortCode = shortCode,
                    ShortUrl = cachedUrl.ShortenUrl
                });
            }
        }

        logger.LogDebug("Cache miss for short code: {ShortCode}, querying database", shortCode);

        // Step 2: Cache miss - query SQL Server
        var shortUrl = await dbContext.ShortUrls
            .FirstOrDefaultAsync(x => x.ShortCode == shortCode, cancellationToken);

        if (shortUrl is null)
        {
            logger.LogWarning("Short code not found: {ShortCode}", shortCode);
            return Results.Problem(
                title: "Short Code Not Found",
                detail: $"No URL found for short code '{shortCode}'.",
                statusCode: StatusCodes.Status404NotFound);
        }

        // Step 3: Repopulate Redis cache
        var cacheMinutes = configuration.GetValue<int>($"{AppConstants.ConfigurationSections.Shortify}:CacheExpirationMinutes", 60);
        var cacheValue = JsonSerializer.Serialize(new CachedShortUrl(shortUrl.OriginalUrl, shortUrl.ShortenUrl));
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheMinutes)
        };

        await cache.SetStringAsync(cacheKey, cacheValue, cacheOptions, cancellationToken);
        logger.LogDebug("Repopulated cache for short code: {ShortCode}, TTL: {CacheMinutes} minutes", shortCode, cacheMinutes);

        logger.LogInformation("Resolved short code: {ShortCode} -> {OriginalUrl}", shortCode, shortUrl.OriginalUrl);

        return Results.Ok(new UnshortifyResponse
        {
            OriginalUrl = shortUrl.OriginalUrl,
            ShortCode = shortCode,
            ShortUrl = shortUrl.ShortenUrl
        });
    }
}
