using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SimoneCappelletti.ShortiFy.Infrastructure.Persistence;

public class ShortiFyDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ShortiFyDbContext>
{
    public ShortiFyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ShortiFyDbContext>();
        var connectionString = "Server=localhost,1433;Database=ShortiFy_Dev;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true";
        optionsBuilder.UseSqlServer(connectionString);
        return new ShortiFyDbContext(optionsBuilder.Options);
    }
}
