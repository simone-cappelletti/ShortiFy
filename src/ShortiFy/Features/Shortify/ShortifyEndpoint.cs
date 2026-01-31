using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using SimoneCappelletti.ShortiFy.Infrastructure.Persistence;
using SimoneCappelletti.ShortiFy.Services;
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
    /// <param name="cacheService">The cache service for Redis operations.</param>
    /// <param name="shortCodeService">The service for generating short codes.</param>
    /// <param name="options">The Shortify configuration options.</param>
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
        IShortUrlCacheService cacheService,
        ShortCodeService shortCodeService,
        IOptions<ShortifyOptions> options,
        ILogger<ShortifyRequest> logger,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing shortify request for URL: {OriginalUrl}", request.OriginalUrl);

        // Validate URL format and scheme
        var validationResult = ValidateUrl(request.OriginalUrl);

        if (validationResult is not null)
        {
            return validationResult;
        }

        // Check if URL already exists (idempotency)
        var existingUrl = await dbContext.ShortUrls
            .FirstOrDefaultAsync(x => x.OriginalUrl == request.OriginalUrl, cancellationToken);

        if (existingUrl is not null)
        {
            logger.LogInformation("URL already exists with short code: {ShortCode}", existingUrl.ShortCode);

            return Results.Ok(new ShortifyResponse(
                existingUrl.ShortCode,
                existingUrl.ShortenUrl,
                existingUrl.OriginalUrl));
        }

        // Generate unique short code with collision handling
        var shortCode = await GenerateUniqueShortCodeAsync();

        if (shortCode is null)
        {
            logger.LogError("Failed to generate unique short code after maximum attempts");

            return Results.Problem(
                title: "Short Code Generation Failed",
                detail: "Unable to generate a unique short code. Please try again.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var config = options.Value;
        var shortenedUrl = $"{config.BaseUrl.TrimEnd('/')}/{shortCode}";

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
        await cacheService.SetAsync(
            shortCode,
            new CachedShortUrl(shortUrl.OriginalUrl, shortUrl.ShortenUrl),
            cancellationToken);

        var response = new ShortifyResponse(shortCode, shortenedUrl, request.OriginalUrl);

        return Results.Created($"/api/unshortify/{shortCode}", response);

        // Local function: Validates URL format and scheme
        IResult? ValidateUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                logger.LogWarning("Invalid URL format: {OriginalUrl}", url);

                return Results.Problem(
                    title: "Invalid URL",
                    detail: "The provided URL is not a valid absolute URL.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (uri.Scheme is not ("http" or "https"))
            {
                logger.LogWarning("Invalid URL scheme: {Scheme} for URL: {OriginalUrl}", uri.Scheme, url);

                return Results.Problem(
                    title: "Invalid URL Scheme",
                    detail: "Only HTTP and HTTPS URLs are supported.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            return null;
        }

        // Local function: Generates a unique short code with collision handling
        async Task<string?> GenerateUniqueShortCodeAsync()
        {
            const int maxAttempts = 10;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var code = shortCodeService.GenerateShortCode();
                var codeExists = await dbContext.ShortUrls
                    .AnyAsync(x => x.ShortCode == code, cancellationToken);

                if (!codeExists)
                {
                    return code;
                }

                logger.LogDebug("Short code collision detected: {ShortCode}, attempt {Attempt}", code, attempt);
            }

            return null;
        }
    }
}
