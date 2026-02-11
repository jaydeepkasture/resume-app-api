using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Domain.Interface;

public interface IBillingRepository
{
    Task<List<MasterValue>> GetMasterValuesByTypeAsync(string masterType);
    Task<List<SubscriptionPlan>> GetActivePlansAsync();
    Task<List<SubscriptionPlanPrice>> GetPricesByPlanIdAsync(long planId);
    Task<UserSubscription?> GetUserSubscriptionAsync(long userId);
    Task<bool> CreateUserSubscriptionAsync(UserSubscription subscription);
    Task<bool> UpdateUserSubscriptionAsync(UserSubscription subscription);
    Task<bool> AddSubscriptionHistoryAsync(UserSubscriptionHistory history);
    Task<bool> CreatePaymentAsync(SubscriptionPayment payment);
    Task<SubscriptionPayment?> GetPaymentByOrderIdAsync(string orderId);
    Task<bool> UpdatePaymentAsync(SubscriptionPayment payment);
    Task<Dictionary<string, string>> GetPlanBenefitsAsync(long planId);
    Task<Dictionary<string, string>> GetUserBenefitsAsync(long userId);
}
