using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

using SimoneCappelletti.ShortiFy.Infrastructure.Persistence;
using SimoneCappelletti.ShortiFy.Services;
using SimoneCappelletti.ShortiFy.Shared.Constants;
using SimoneCappelletti.ShortiFy.Shared.Models;

namespace SimoneCappelletti.ShortiFy.Features.Shortify;

/// <summary>
/// Endpoint for creating shortened URLs.
/// </summary>
public static class ShortifyEndpoint
{
    /// <summary>
    /// Handles the POST /api/shortify request to create a shortened URL.
    /// Implements idempotency - returns existing short URL if the original URL was previously shortened.
    /// </summary>
    /// <param name="request">The shortify request containing the original URL.</param>
    /// <param name="dbContext">The database context for persistence.</param>
    /// <param name="cache">The distributed cache for Redis operations.</param>
    /// <param name="shortCodeService">The service for generating short codes.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="logger">The logger for structured logging.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>
    /// 200 OK with ShortifyResponse if URL already exists (idempotent).
    /// 201 Created with ShortifyResponse for new URLs.
    /// 400 Bad Request with Problem Details for invalid URLs.
    /// </returns>
    public static async Task<IResult> HandleAsync(
        ShortifyRequest request,
        ShortiFyDbContext dbContext,
        IDistributedCache cache,
        ShortCodeService shortCodeService,
        IConfiguration configuration,
        ILogger<ShortifyRequest> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing shortify request for URL: {OriginalUrl}", request.OriginalUrl);

        // Validate URL format and scheme
        if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out var uri))
        {
            logger.LogWarning("Invalid URL format: {OriginalUrl}", request.OriginalUrl);
            return Results.Problem(
                title: "Invalid URL",
                detail: "The provided URL is not a valid absolute URL.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (uri.Scheme is not ("http" or "https"))
        {
            logger.LogWarning("Invalid URL scheme: {Scheme} for URL: {OriginalUrl}", uri.Scheme, request.OriginalUrl);
            return Results.Problem(
                title: "Invalid URL Scheme",
                detail: "Only HTTP and HTTPS URLs are supported.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        // Check if URL already exists (idempotency)
        var existingUrl = await dbContext.ShortUrls
            .FirstOrDefaultAsync(x => x.OriginalUrl == request.OriginalUrl, cancellationToken);

        if (existingUrl is not null)
        {
            logger.LogInformation("URL already exists with short code: {ShortCode}", existingUrl.ShortCode);
            return Results.Ok(new ShortifyResponse
            {
                ShortCode = existingUrl.ShortCode,
                ShortUrl = existingUrl.ShortenUrl,
                OriginalUrl = existingUrl.OriginalUrl
            });
        }

        // Get configuration
        var baseUrl = configuration.GetValue<string>($"{AppConstants.ConfigurationSections.Shortify}:BaseUrl")
            ?? throw new InvalidOperationException("Shortify:BaseUrl configuration is missing.");
        var cacheMinutes = configuration.GetValue<int>($"{AppConstants.ConfigurationSections.Shortify}:CacheExpirationMinutes", 60);

        // Generate unique short code with collision handling
        string shortCode;
        var maxAttempts = 10;
        var attempt = 0;

        do
        {
            shortCode = shortCodeService.GenerateShortCode();
            attempt++;

            var codeExists = await dbContext.ShortUrls
                .AnyAsync(x => x.ShortCode == shortCode, cancellationToken);

            if (!codeExists)
            {
                break;
            }

            logger.LogDebug("Short code collision detected: {ShortCode}, attempt {Attempt}", shortCode, attempt);
        }
        while (attempt < maxAttempts);

        if (attempt >= maxAttempts)
        {
            logger.LogError("Failed to generate unique short code after {MaxAttempts} attempts", maxAttempts);
            return Results.Problem(
                title: "Short Code Generation Failed",
                detail: "Unable to generate a unique short code. Please try again.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var shortenedUrl = $"{baseUrl.TrimEnd('/')}/{shortCode}";

        // Create and persist the short URL
        var shortUrl = new ShortUrl
        {
            ShortCode = shortCode,
            OriginalUrl = request.OriginalUrl,
            ShortenUrl = shortenedUrl
        };

        dbContext.ShortUrls.Add(shortUrl);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created short URL: {ShortCode} -> {OriginalUrl}", shortCode, request.OriginalUrl);

        // Cache the URL for fast lookups
        var cacheKey = $"{AppConstants.CacheKeys.ShortifyPrefix}{shortCode}";
        var cacheValue = JsonSerializer.Serialize(new CachedShortUrl(shortUrl.OriginalUrl, shortUrl.ShortenUrl));
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheMinutes)
        };

        await cache.SetStringAsync(cacheKey, cacheValue, cacheOptions, cancellationToken);
        logger.LogDebug("Cached short URL with key: {CacheKey}, TTL: {CacheMinutes} minutes", cacheKey, cacheMinutes);

        var response = new ShortifyResponse
        {
            ShortCode = shortCode,
            ShortUrl = shortenedUrl,
            OriginalUrl = request.OriginalUrl
        };

        return Results.Created($"/api/unshortify/{shortCode}", response);
    }
}

/// <summary>
/// Internal record for caching short URL data in Redis.
/// </summary>
/// <param name="OriginalUrl">The original URL.</param>
/// <param name="ShortenUrl">The shortened URL.</param>
internal sealed record CachedShortUrl(string OriginalUrl, string ShortenUrl);
