using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;
using ResumeInOneMinute.Repository.DataContexts;

namespace ResumeInOneMinute.Repository.Repositories;

public class BillingRepository : BaseRepository, IBillingRepository
{
    public BillingRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<List<MasterValue>> GetMasterValuesByTypeAsync(string masterType)
    {
        using (var context = CreateDbContext())
        {
            return await context.MasterValues
                .Where(m => m.MasterType == masterType && m.IsActive)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();
        }
    }

    public async Task<List<SubscriptionPlan>> GetActivePlansAsync()
    {
        using (var context = CreateDbContext())
        {
            return await context.SubscriptionPlans
                .Include(p => p.PlanCode)
                .Where(p => p.IsActive)
                .ToListAsync();
        }
    }

    public async Task<List<SubscriptionPlanPrice>> GetPricesByPlanIdAsync(long planId)
    {
        using (var context = CreateDbContext())
        {
            return await context.SubscriptionPlanPrices
                .Include(p => p.BillingCycle)
                .Include(p => p.Currency)
                .Where(p => p.PlanId == planId && p.IsActive)
                .ToListAsync();
        }
    }

    public async Task<UserSubscription?> GetUserSubscriptionAsync(long userId)
    {
        using (var context = CreateDbContext())
        {
            return await context.UserSubscriptions
                .Include(s => s.Plan)
                .Include(s => s.PlanPrice)
                .Include(s => s.Status)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }
    }

    public async Task<bool> CreateUserSubscriptionAsync(UserSubscription subscription)
    {
        using (var context = CreateDbContext())
        {
            context.UserSubscriptions.Add(subscription);
            return await context.SaveChangesAsync() > 0;
        }
    }

    public async Task<bool> UpdateUserSubscriptionAsync(UserSubscription subscription)
    {
        using (var context = CreateDbContext())
        {
            subscription.UpdatedAt = DateTime.UtcNow;
            context.UserSubscriptions.Update(subscription);
            return await context.SaveChangesAsync() > 0;
        }
    }

    public async Task<bool> AddSubscriptionHistoryAsync(UserSubscriptionHistory history)
    {
        using (var context = CreateDbContext())
        {
            context.UserSubscriptionHistories.Add(history);
            return await context.SaveChangesAsync() > 0;
        }
    }

    public async Task<bool> CreatePaymentAsync(SubscriptionPayment payment)
    {
        using (var context = CreateDbContext())
        {
            context.SubscriptionPayments.Add(payment);
            return await context.SaveChangesAsync() > 0;
        }
    }

    public async Task<SubscriptionPayment?> GetPaymentByOrderIdAsync(string orderId)
    {
        using (var context = CreateDbContext())
        {
            return await context.SubscriptionPayments
                .FirstOrDefaultAsync(p => p.ProviderOrderId == orderId);
        }
    }

    public async Task<bool> UpdatePaymentAsync(SubscriptionPayment payment)
    {
        using (var context = CreateDbContext())
        {
            context.SubscriptionPayments.Update(payment);
            return await context.SaveChangesAsync() > 0;
        }
    }

    public async Task<Dictionary<string, string>> GetPlanBenefitsAsync(long planId)
    {
        using (var context = CreateDbContext())
        {
            return await context.PlanBenefitMaps
                .Where(m => m.PlanId == planId)
                .Include(m => m.Benefit)
                .ToDictionaryAsync(
                    m => m.Benefit?.BenefitCode ?? string.Empty,
                    m => m.BenefitValue
                );
        }
    }

    public async Task<Dictionary<string, string>> GetUserBenefitsAsync(long userId)
    {
        using (var context = CreateDbContext())
        {
            var activeSubscription = await context.UserSubscriptions
                .Include(s => s.Status)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status != null && s.Status.Code == "ACTIVE");

            if (activeSubscription == null) return new Dictionary<string, string>();

            return await GetPlanBenefitsAsync(activeSubscription.PlanId);
        }
    }
}
