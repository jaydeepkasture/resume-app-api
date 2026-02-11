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
    public DbSet<MasterValue> MasterValues { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<SubscriptionPlanPrice> SubscriptionPlanPrices { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }
    public DbSet<UserSubscriptionHistory> UserSubscriptionHistories { get; set; }
    public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }
    public DbSet<PlanBenefit> PlanBenefits { get; set; }
    public DbSet<PlanBenefitMap> PlanBenefitMaps { get; set; }

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

        // Master Values
        modelBuilder.Entity<MasterValue>(entity =>
        {
            entity.HasIndex(e => new { e.MasterType, e.Code })
                .IsUnique();
                
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(NOW() AT TIME ZONE 'UTC')");
        });

        // Subscription Plans
        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(NOW() AT TIME ZONE 'UTC')");
        });

        // Subscription Plan Prices
        modelBuilder.Entity<SubscriptionPlanPrice>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(NOW() AT TIME ZONE 'UTC')");
        });

        // User Subscriptions
        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.HasIndex(e => e.UserId)
                .IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(NOW() AT TIME ZONE 'UTC')");
        });

        // User Subscription History
        modelBuilder.Entity<UserSubscriptionHistory>(entity =>
        {
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("(NOW() AT TIME ZONE 'UTC')");
        });

        // Subscription Payments
        modelBuilder.Entity<SubscriptionPayment>(entity =>
        {
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(NOW() AT TIME ZONE 'UTC')");
        });

        // Seed Master Data
        modelBuilder.Entity<MasterValue>().HasData(
            // PLAN_CODE
            new MasterValue { MasterValueId = 1, MasterType = "PLAN_CODE", Code = "FREE", DisplayName = "Free", SortOrder = 1 },
            new MasterValue { MasterValueId = 2, MasterType = "PLAN_CODE", Code = "PRO", DisplayName = "Pro", SortOrder = 2 },
            new MasterValue { MasterValueId = 3, MasterType = "PLAN_CODE", Code = "ENTERPRISE", DisplayName = "Enterprise", SortOrder = 3 },

            // BILLING_CYCLE
            new MasterValue { MasterValueId = 4, MasterType = "BILLING_CYCLE", Code = "MONTHLY", DisplayName = "Monthly", SortOrder = 1 },
            new MasterValue { MasterValueId = 5, MasterType = "BILLING_CYCLE", Code = "YEARLY", DisplayName = "Yearly", SortOrder = 2 },

            // CURRENCY
            new MasterValue { MasterValueId = 6, MasterType = "CURRENCY", Code = "INR", DisplayName = "Indian Rupee", SortOrder = 1 },
            new MasterValue { MasterValueId = 7, MasterType = "CURRENCY", Code = "USD", DisplayName = "US Dollar", SortOrder = 2 },

            // SUBSCRIPTION_STATUS
            new MasterValue { MasterValueId = 8, MasterType = "SUBSCRIPTION_STATUS", Code = "ACTIVE", DisplayName = "Active", SortOrder = 1 },
            new MasterValue { MasterValueId = 9, MasterType = "SUBSCRIPTION_STATUS", Code = "CANCELLED", DisplayName = "Cancelled", SortOrder = 2 },
            new MasterValue { MasterValueId = 10, MasterType = "SUBSCRIPTION_STATUS", Code = "EXPIRED", DisplayName = "Expired", SortOrder = 3 },
            new MasterValue { MasterValueId = 11, MasterType = "SUBSCRIPTION_STATUS", Code = "PAUSED", DisplayName = "Paused", SortOrder = 4 },

            // SUBSCRIPTION_CHANGE_TYPE
            new MasterValue { MasterValueId = 12, MasterType = "SUBSCRIPTION_CHANGE_TYPE", Code = "UPGRADE", DisplayName = "Upgrade", SortOrder = 1 },
            new MasterValue { MasterValueId = 13, MasterType = "SUBSCRIPTION_CHANGE_TYPE", Code = "DOWNGRADE", DisplayName = "Downgrade", SortOrder = 2 },
            new MasterValue { MasterValueId = 14, MasterType = "SUBSCRIPTION_CHANGE_TYPE", Code = "RENEWAL", DisplayName = "Renewal", SortOrder = 3 },
            new MasterValue { MasterValueId = 15, MasterType = "SUBSCRIPTION_CHANGE_TYPE", Code = "CANCEL", DisplayName = "Cancel", SortOrder = 4 },

            // PAYMENT_STATUS
            new MasterValue { MasterValueId = 16, MasterType = "PAYMENT_STATUS", Code = "PENDING", DisplayName = "Pending", SortOrder = 1 },
            new MasterValue { MasterValueId = 17, MasterType = "PAYMENT_STATUS", Code = "SUCCESS", DisplayName = "Success", SortOrder = 2 },
            new MasterValue { MasterValueId = 18, MasterType = "PAYMENT_STATUS", Code = "FAILED", DisplayName = "Failed", SortOrder = 3 },

            // PAYMENT_PROVIDER
            new MasterValue { MasterValueId = 19, MasterType = "PAYMENT_PROVIDER", Code = "RAZORPAY", DisplayName = "Razorpay", SortOrder = 1 }
        );

        // Seed Benefits
        modelBuilder.Entity<PlanBenefit>().HasData(
            new PlanBenefit { BenefitId = 1, BenefitCode = "TEMPLATE_LIMIT", BenefitName = "Template Limit", Description = "Number of templates allowed" },
            new PlanBenefit { BenefitId = 2, BenefitCode = "RATE_LIMIT_PER_MINUTE", BenefitName = "Rate Limit", Description = "API requests per minute" },
            new PlanBenefit { BenefitId = 3, BenefitCode = "DAILY_TOKEN_LIMIT", BenefitName = "Daily Token Limit", Description = "Total instruction message characters per day" }
        );

        // Seed Plan-Benefit Mappings
        modelBuilder.Entity<PlanBenefitMap>().HasData(
            // FREE Plan (Assuming PlanId 1 for FREE)
            new PlanBenefitMap { MapId = 1, PlanId = 1, BenefitId = 1, BenefitValue = "3" },
            new PlanBenefitMap { MapId = 2, PlanId = 1, BenefitId = 2, BenefitValue = "30" },
            new PlanBenefitMap { MapId = 5, PlanId = 1, BenefitId = 3, BenefitValue = "3000" },
            // PRO Plan (Assuming PlanId 2 for PRO)
            new PlanBenefitMap { MapId = 3, PlanId = 2, BenefitId = 1, BenefitValue = "50" },
            new PlanBenefitMap { MapId = 4, PlanId = 2, BenefitId = 2, BenefitValue = "300" },
            new PlanBenefitMap { MapId = 6, PlanId = 2, BenefitId = 3, BenefitValue = "10000" }
        );
    }
}
