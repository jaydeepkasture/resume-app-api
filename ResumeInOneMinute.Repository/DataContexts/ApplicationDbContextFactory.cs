using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;


namespace ResumeInOneMinute.Repository.DataContexts;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Load configuration to get the connection string
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory())?.FullName 
                         ?? System.IO.Directory.GetCurrentDirectory())

            .AddJsonFile("ResumeInOneMinute/appsettings.json", optional: true)
            .AddJsonFile($"ResumeInOneMinute/appsettings.{environmentName}.json", optional: true)
            .AddJsonFile("ResumeInOneMinute/appsettings.Shared.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("PostgreSQL");

        optionsBuilder.UseNpgsql(connectionString)
                      .UseSnakeCaseNamingConvention();


        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
