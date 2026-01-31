namespace SimoneCappelletti.ShortiFy.Services;

/// <summary>
/// Service interface for generating and managing short codes.
/// </summary>
public interface IShortCodeService
{
    /// <summary>
    /// Default length of generated short codes.
    /// </summary>
    const int DefaultCodeLength = 6;

    /// <summary>
    /// Generates a cryptographically random short code.
    /// </summary>
    /// <param name="length">The length of the short code to generate. Defaults to 6.</param>
    /// <returns>A random Base62 encoded short code.</returns>
    /// <example>
    /// <code>
    /// var service = new ShortCodeService();
    /// var shortCode = service.GenerateShortCode(); // e.g., "aBc1Xy"
    /// </code>
    /// </example>
    string GenerateShortCode(int length = DefaultCodeLength);
}
