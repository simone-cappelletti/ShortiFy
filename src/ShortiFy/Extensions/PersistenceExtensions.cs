using Microsoft.EntityFrameworkCore;

using SimoneCappelletti.ShortiFy.Infrastructure.Persistence;

namespace SimoneCappelletti.ShortiFy.Extensions;

/// <summary>
/// Extension methods for configuring Entity Framework Core persistence.
/// </summary>
public static class PersistenceExtensions
{
    /// <summary>
    /// Configures EF Core with SQL Server and retry logic for transient failures.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when connection string is not configured.</exception>
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ShortiFyDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        return services;
    }
}
