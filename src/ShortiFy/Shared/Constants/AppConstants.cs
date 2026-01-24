namespace SimoneCappelletti.ShortiFy.Shared.Constants;

/// <summary>
/// Application-wide constants to avoid hard-coded strings throughout the codebase.
/// </summary>
public sealed class AppConstants
{
    /// <summary>
    /// The application name used for service identification, logging, and telemetry.
    /// </summary>
    public const string ApplicationName = "ShortiFy";

    /// <summary>
    /// Connection string names used in configuration.
    /// </summary>
    public sealed class ConnectionStrings
    {
        /// <summary>
        /// The default SQL Server connection string name.
        /// </summary>
        public const string DefaultConnection = "DefaultConnection";

        /// <summary>
        /// The Redis cache connection string name.
        /// </summary>
        public const string Redis = "Redis";
    }

    /// <summary>
    /// Environment variable names used by the application.
    /// </summary>
    public sealed class EnvironmentVariables
    {
        /// <summary>
        /// The ASP.NET Core environment variable name.
        /// </summary>
        public const string AspNetCoreEnvironment = "ASPNETCORE_ENVIRONMENT";
    }

    /// <summary>
    /// Default environment names.
    /// </summary>
    public sealed class Environments
    {
        /// <summary>
        /// The development environment name.
        /// </summary>
        public const string Development = "Development";
    }
}
