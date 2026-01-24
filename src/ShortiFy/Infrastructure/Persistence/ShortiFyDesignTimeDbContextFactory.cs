using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using SimoneCappelletti.ShortiFy.Shared.Constants;

namespace SimoneCappelletti.ShortiFy.Infrastructure.Persistence;

/// <summary>
/// Factory for creating <see cref="ShortiFyDbContext"/> instances at design time.
/// Used by EF Core tools (e.g., dotnet ef migrations) when running outside the app's
/// normal startup configuration.
/// </summary>
public class ShortiFyDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ShortiFyDbContext>
{
    public ShortiFyDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable(AppConstants.EnvironmentVariables.AspNetCoreEnvironment)
            ?? AppConstants.Environments.Development;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString(AppConstants.ConnectionStrings.DefaultConnection)
            ?? throw new InvalidOperationException(
                $"Connection string '{AppConstants.ConnectionStrings.DefaultConnection}' not found. Ensure appsettings.json or appsettings.{environment}.json contains the connection string. Current environment: {environment}");

        var optionsBuilder = new DbContextOptionsBuilder<ShortiFyDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new ShortiFyDbContext(optionsBuilder.Options);
    }
}

