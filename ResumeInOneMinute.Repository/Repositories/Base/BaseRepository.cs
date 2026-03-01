using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ResumeInOneMinute.Repository.DataContexts;
using Npgsql;
using System.Text.Json;

namespace ResumeInOneMinute.Repository.Repositories.Base;

/// <summary>
/// Base repository class providing common database context management functionality
/// </summary>
public abstract class BaseRepository
{
    protected readonly IConfiguration Configuration;
    private readonly string _connectionString;
    private static NpgsqlDataSource? _dataSource;
    private static readonly object Lock = new();

    protected BaseRepository(IConfiguration configuration)
    {
        Configuration = configuration;
        _connectionString = configuration.GetConnectionString("PostgreSQL") 
            ?? throw new InvalidOperationException("PostgreSQL connection string not configured");
        
        if (_dataSource == null)
        {
            lock (Lock)
            {
                if (_dataSource == null)
                {
                    var builder = new NpgsqlDataSourceBuilder(_connectionString);
                    // Fallback to basic EnableDynamicJson and use [JsonPropertyName] in DTO instead
                    // as the current Npgsql version/setup seems to have restricted overloads
                    builder.EnableDynamicJson();
                    _dataSource = builder.Build();
                }
            }
        }
    }

    /// <summary>
    /// Creates a new instance of ApplicationDbContext with manual control
    /// </summary>
    protected ApplicationDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(_dataSource!)
                      .UseSnakeCaseNamingConvention();
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
