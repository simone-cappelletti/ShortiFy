using System.Security.Cryptography;
using System.Text;

namespace SimoneCappelletti.ShortiFy.Services;

/// <summary>
/// Service for generating unique short codes using Base62 encoding.
/// </summary>
public sealed class ShortCodeService
{
    /// <summary>
    /// Base62 character set: a-z, A-Z, 0-9 (62 characters total).
    /// Provides 62^6 = ~56 billion unique combinations for 6-character codes.
    /// </summary>
    private const string Base62Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    /// <summary>
    /// Default length of generated short codes.
    /// </summary>
    private const int DefaultCodeLength = 6;

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
    public string GenerateShortCode(int length = DefaultCodeLength)
    {
        var stringBuilder = new StringBuilder(length);

        for (var i = 0; i < length; i++)
        {
            var randomIndex = RandomNumberGenerator.GetInt32(Base62Chars.Length);
            stringBuilder.Append(Base62Chars[randomIndex]);
        }

        return stringBuilder.ToString();
    }
}
