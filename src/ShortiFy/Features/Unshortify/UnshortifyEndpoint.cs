using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using SimoneCappelletti.ShortiFy.Features.Shortify;
using SimoneCappelletti.ShortiFy.Infrastructure.Persistence;
using SimoneCappelletti.ShortiFy.Services;

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
    /// <param name="cacheService">The cache service for Redis operations.</param>
    /// <param name="options">The Shortify configuration options.</param>
    /// <param name="logger">The logger for structured logging.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>
    /// 200 OK with UnshortifyResponse if found.
    /// 404 Not Found with Problem Details if short code doesn't exist.
    /// </returns>
    public static async Task<IResult> HandleAsync(
        string shortCode,
        ShortiFyDbContext dbContext,
        IShortUrlCacheService cacheService,
        IOptions<ShortifyOptions> options,
        ILogger<UnshortifyResponse> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing unshortify request for short code: {ShortCode}", shortCode);

        // Step 1: Check Redis cache first
        var cachedUrl = await cacheService.GetAsync(shortCode, cancellationToken);

        if (cachedUrl is not null)
        {
            return Results.Ok(new UnshortifyResponse(
                cachedUrl.OriginalUrl,
                shortCode,
                cachedUrl.ShortenUrl));
        }

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
        await cacheService.SetAsync(
            shortCode,
            new CachedShortUrl(shortUrl.OriginalUrl, shortUrl.ShortenUrl),
            cancellationToken);

        logger.LogInformation("Resolved short code: {ShortCode} -> {OriginalUrl}", shortCode, shortUrl.OriginalUrl);

        return Results.Ok(new UnshortifyResponse(
            shortUrl.OriginalUrl,
            shortCode,
            shortUrl.ShortenUrl));
    }
}
