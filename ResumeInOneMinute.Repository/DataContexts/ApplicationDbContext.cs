using Microsoft.EntityFrameworkCore;
using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Repository.DataContexts;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            
            entity.Property(e => e.GlobalUserId)
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(NOW() AT TIME ZONE 'UTC')");

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.HasIndex(e => e.GlobalUserId)
                .IsUnique();

            // Configure relationship
            entity.HasOne(e => e.UserProfile)
                .WithOne(e => e.User)
                .HasForeignKey<UserProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserProfile entity
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserProfileId);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(NOW() AT TIME ZONE 'UTC')");
            
            entity.Property(e => e.GlobalUserProfileId)
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("gen_random_uuid()");
        });
    }
}
