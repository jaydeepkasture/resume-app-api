using ResumeInOneMinute.Domain.DTO;

namespace ResumeInOneMinute.Domain.Interface;

public interface ISubscriptionService
{
    Task<List<SubscriptionPlanDto>> GetAvailablePlansAsync();
    Task<UserSubscriptionDto?> GetUserSubscriptionAsync(long userId);
    Task<RazorpayOrderResponseDto> CreatePaymentOrderAsync(long userId, CreateOrderRequestDto request);
    Task<SubscriptionPlanDto?> GetPlanByIdAsync(long planId);
    Task<SubscriptionConfirmResponseDto?> ConfirmPaymentAsync(long userId, ConfirmPaymentRequestDto request);
    Task<Dictionary<string, string>> GetUserBenefitsAsync(long userId);
}
