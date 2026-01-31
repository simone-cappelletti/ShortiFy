using System.Security.Cryptography;
using System.Text;

namespace SimoneCappelletti.ShortiFy.Services;

/// <summary>
/// Service for generating unique short codes using Base62 encoding.
/// </summary>
public sealed class ShortCodeService : IShortCodeService
{
    /// <summary>
    /// Base62 character set: a-z, A-Z, 0-9 (62 characters total).
    /// Provides 62^6 = ~56 billion unique combinations for 6-character codes.
    /// </summary>
    private const string Base62Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    /// <inheritdoc/>
    public string GenerateShortCode(int length = IShortCodeService.DefaultCodeLength)
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
