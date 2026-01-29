using Microsoft.EntityFrameworkCore;

using SimoneCappelletti.ShortiFy.Shared.Models;

namespace SimoneCappelletti.ShortiFy.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context for ShortiFy.
/// </summary>
public sealed class ShortiFyDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShortiFyDbContext"/> class.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    public ShortiFyDbContext(DbContextOptions<ShortiFyDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the ShortUrls DbSet.
    /// </summary>
    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ShortUrl>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.ShortCode)
                  .IsUnique();

            entity.Property(e => e.ShortCode)
                  .HasMaxLength(10)
                  .IsRequired();

            entity.Property(e => e.OriginalUrl)
                  .HasMaxLength(2048)
                  .IsRequired();

            entity.Property(e => e.ShortenUrl)
                .HasMaxLength(2048)
                .IsRequired();

            entity.Property(e => e.CreatedOnUtc)
                  .HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
