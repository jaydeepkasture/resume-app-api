using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ResumeInOneMinute.Repository.DataContexts;

namespace ResumeInOneMinute.Repository.Repositories;

/// <summary>
/// Base repository class providing common database context management functionality
/// </summary>
public abstract class BaseRepository
{
    protected readonly IConfiguration Configuration;
    private readonly string _connectionString;

    protected BaseRepository(IConfiguration configuration)
    {
        Configuration = configuration;
        _connectionString = configuration.GetConnectionString("PostgreSQL") 
            ?? throw new InvalidOperationException("PostgreSQL connection string not configured");
    }

    /// <summary>
    /// Creates a new instance of ApplicationDbContext with manual control
    /// </summary>
    protected ApplicationDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(_connectionString)
                      .UseSnakeCaseNamingConvention();
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
