using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimoneCappelletti.ShortiFy.Shared.Models;

/// <summary>
/// Represents a shortened URL entity stored in the database.
/// </summary>
[Table("ShortUrls")]
public class ShortUrl
{
    /// <summary>
    /// Gets or sets the primary key identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique short code (e.g., "aBc1Xy").
    /// </summary>
    [Required]
    [StringLength(10)]
    public required string ShortCode { get; set; }

    /// <summary>
    /// Gets or sets the original full URL.
    /// </summary>
    [Required]
    [MaxLength(2048)]
    public required string OriginalUrl { get; set; }

    /// <summary>
    /// Gets or sets the shortened URL.
    /// </summary>
    [Required]
    [MaxLength(2048)]
    public required string ShortenUrl { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the URL was created.
    /// </summary>
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
}
