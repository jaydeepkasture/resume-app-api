using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ResumeInOneMinute.Repository.DataContexts;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Use the connection string directly to avoid path issues during migration
        var connectionString = "Host=localhost;Port=5432;Database=resume_app_dev;Username=postgres;Password=htmltopdffile@007";

        optionsBuilder.UseNpgsql(connectionString)
                      .UseSnakeCaseNamingConvention();

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
