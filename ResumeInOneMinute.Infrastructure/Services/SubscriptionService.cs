using ResumeInOneMinute.Domain.Constants;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly IBillingRepository _billingRepository;
    private readonly IRazorpayService _razorpayService;
    private readonly IAccountRepository _accountRepository;

    public SubscriptionService(
        IBillingRepository billingRepository, 
        IRazorpayService razorpayService,
        IAccountRepository accountRepository)
    {
        _billingRepository = billingRepository;
        _razorpayService = razorpayService;
        _accountRepository = accountRepository;
    }

    public async Task<List<SubscriptionPlanDto>> GetAvailablePlansAsync()
    {
        var plans = await _billingRepository.GetActivePlansAsync();
        var result = new List<SubscriptionPlanDto>();

        foreach (var plan in plans)
        {
            var prices = await _billingRepository.GetPricesByPlanIdAsync(plan.PlanId);
            var benefits = await _billingRepository.GetPlanBenefitsAsync(plan.PlanId);
            
            result.Add(new SubscriptionPlanDto
            {
                PlanId = plan.PlanId,
                PlanCode = plan.PlanCode?.Code ?? string.Empty,
                PlanName = plan.PlanName,
                Description = plan.Description,
                Prices = prices.Select(p => new SubscriptionPriceDto
                {
                    PlanPriceId = p.PlanPriceId,
                    BillingCycle = p.BillingCycle?.Code ?? string.Empty,
                    Currency = p.Currency?.Code ?? string.Empty,
                    Price = p.Price
                }).ToList(),
                Benefits = benefits
            });
        }

        return result;
    }

    public async Task<SubscriptionPlanDto?> GetPlanByIdAsync(long planId)
    {
        var plans = await GetAvailablePlansAsync();
        return plans.FirstOrDefault(p => p.PlanId == planId);
    }

    public async Task<UserSubscriptionDto?> GetUserSubscriptionAsync(long userId)
    {
        var sub = await _billingRepository.GetUserSubscriptionAsync(userId);
        if (sub == null) return null;

        var benefits = await _billingRepository.GetUserBenefitsAsync(userId);

        return new UserSubscriptionDto
        {
            PlanName = sub.Plan?.PlanName ?? "Unknown",
            Status = sub.Status?.Code ?? "UNKNOWN",
            StartDate = sub.StartDate,
            EndDate = sub.EndDate,
            Benefits = benefits
        };
    }

    public async Task<RazorpayOrderResponseDto> CreatePaymentOrderAsync(long userId, CreateOrderRequestDto request)
    {
        // 1. Get plan details from DB
        var plans = await _billingRepository.GetActivePlansAsync();
        var selectedPlan = plans.FirstOrDefault(p => p.PlanCode?.Code == request.PlanPriceId);

        if (selectedPlan == null) throw new Exception($"Invalid Plan Code: {request.PlanPriceId}");

        var prices = await _billingRepository.GetPricesByPlanIdAsync(selectedPlan.PlanId);
        var selectedPrice = prices.FirstOrDefault(p => p.BillingCycle?.Code == request.BillingCycle);

        if (selectedPrice == null) throw new Exception($"Invalid Billing Cycle: {request.BillingCycle} for plan {request.PlanPriceId}");

        // If price is 0, activate immediately
        if (selectedPrice.Price == 0)
        {
            await ActivateSubscriptionAsync(userId, selectedPlan!, selectedPrice, "FREE_ACTIVATION");
            return new RazorpayOrderResponseDto
            {
                OrderId = "FREE_PLAN",
                Amount = 0,
                Currency = selectedPrice.Currency?.Code ?? "INR"
            };
        }

        // 2. Create Razorpay Order
        var orderId = _razorpayService.CreateOrder(selectedPrice.Price, selectedPrice.Currency?.Code ?? "INR", $"receipt_{userId}_{DateTime.UtcNow.Ticks}");

        // 3. Log pending payment
        var paymentStatus = (await _billingRepository.GetMasterValuesByTypeAsync(BillingConstants.MasterTypes.PaymentStatus))
            .First(m => m.Code == BillingConstants.PaymentStatuses.Pending);
        
        var paymentProvider = (await _billingRepository.GetMasterValuesByTypeAsync(BillingConstants.MasterTypes.PaymentProvider))
            .First(m => m.Code == BillingConstants.PaymentProviders.Razorpay);

        var payment = new SubscriptionPayment
        {
            PaymentId = Guid.NewGuid(),
            PaymentProviderId = paymentProvider.MasterValueId,
            PaymentStatusId = paymentStatus.MasterValueId,
            PlanPriceId = selectedPrice.PlanPriceId,
            Amount = selectedPrice.Price,
            CurrencyId = selectedPrice.CurrencyId,
            ProviderOrderId = orderId,
            CreatedAt = DateTime.UtcNow
        };

        await _billingRepository.CreatePaymentAsync(payment);

        return new RazorpayOrderResponseDto
        {
            OrderId = orderId,
            Amount = selectedPrice.Price,
            Currency = selectedPrice.Currency?.Code ?? "INR"
        };
    }

    public async Task<SubscriptionConfirmResponseDto?> ConfirmPaymentAsync(long userId, ConfirmPaymentRequestDto request)
    {
        // 1. Verify signature
        bool isValid = _razorpayService.VerifyPayment(request.RazorpayPaymentId, request.RazorpayOrderId, request.RazorpaySignature);
        if (!isValid) return null;

        // 2. Fetch payment record with plan price details
        var payment = await _billingRepository.GetPaymentByOrderIdAsync(request.RazorpayOrderId);
        if (payment == null || payment.PaymentStatusId != (await GetPaymentStatusId(BillingConstants.PaymentStatuses.Pending))) 
            return null;

        // 3. Update payment status to Success
        payment.PaymentStatusId = await GetPaymentStatusId(BillingConstants.PaymentStatuses.Success);
        payment.ProviderPaymentId = request.RazorpayPaymentId;
        payment.PaidAt = DateTime.UtcNow;

        await _billingRepository.UpdatePaymentAsync(payment);

        // 4. Fetch User and Plan details
        var user = await _accountRepository.GetUserByIdAsync(userId);
        if (user == null) return null;

        var plans = await _billingRepository.GetActivePlansAsync();
        SubscriptionPlanPrice? priceRecord = null;
        SubscriptionPlan? planRecord = null;

        foreach (var p in plans)
        {
            var prices = await _billingRepository.GetPricesByPlanIdAsync(p.PlanId);
            priceRecord = prices.FirstOrDefault(pr => pr.PlanPriceId == payment.PlanPriceId);
            if (priceRecord != null)
            {
                planRecord = p;
                break;
            }
        }

        if (priceRecord == null || planRecord == null) return null;

        // 5. Create or Update Subscription
        await ActivateSubscriptionAsync(userId, planRecord, priceRecord, "RAZORPAY_PAYMENT");

        var updatedSub = await _billingRepository.GetUserSubscriptionAsync(userId);
        if (updatedSub != null)
        {
            payment.UserSubscriptionId = updatedSub.UserSubscriptionId;
            await _billingRepository.UpdatePaymentAsync(payment);
        }

        var benefits = await _billingRepository.GetPlanBenefitsAsync(planRecord.PlanId);

        return new SubscriptionConfirmResponseDto
        {
            Status = "ACTIVE",
            Plan = planRecord.PlanCode?.Code ?? "PRO",
            Benefits = benefits
        };
    }

    public async Task<Dictionary<string, string>> GetUserBenefitsAsync(long userId)
    {
        return await _billingRepository.GetUserBenefitsAsync(userId);
    }

    private async Task ActivateSubscriptionAsync(long userId, SubscriptionPlan plan, SubscriptionPlanPrice price, string source, Guid? paymentId = null)
    {
        var user = await _accountRepository.GetUserByIdAsync(userId);
        if (user == null) return;

        var existingSub = await _billingRepository.GetUserSubscriptionAsync(userId);
        bool isNewSubscription = existingSub == null;
        long? oldPlanId = existingSub?.PlanId;
        long? oldPlanPriceId = existingSub?.PlanPriceId;

        var activeStatus = (await _billingRepository.GetMasterValuesByTypeAsync(BillingConstants.MasterTypes.SubscriptionStatus))
            .First(m => m.Code == BillingConstants.SubscriptionStatuses.Active);

        var cycleCode = price.BillingCycle?.Code ?? BillingConstants.BillingCycles.Monthly;
        var durationDays = cycleCode == BillingConstants.BillingCycles.Yearly ? 365 : 30;

        if (existingSub == null)
        {
            existingSub = new UserSubscription
            {
                UserSubscriptionId = Guid.NewGuid(),
                UserId = userId,
                GlobalUserId = user.GlobalUserId,
                PlanId = plan.PlanId,
                PlanPriceId = price.PlanPriceId,
                StatusId = activeStatus.MasterValueId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(durationDays),
                CreatedAt = DateTime.UtcNow
            };
            await _billingRepository.CreateUserSubscriptionAsync(existingSub);
        }
        else
        {
            existingSub.PlanId = plan.PlanId;
            existingSub.PlanPriceId = price.PlanPriceId;
            existingSub.StatusId = activeStatus.MasterValueId;
            existingSub.StartDate = DateTime.UtcNow;
            existingSub.EndDate = DateTime.UtcNow.AddDays(durationDays);
            await _billingRepository.UpdateUserSubscriptionAsync(existingSub);
        }

        var changeTypeCode = isNewSubscription ? BillingConstants.SubscriptionChangeTypes.Upgrade : BillingConstants.SubscriptionChangeTypes.Renewal;
        var changeType = (await _billingRepository.GetMasterValuesByTypeAsync(BillingConstants.MasterTypes.SubscriptionChangeType))
            .First(m => m.Code == changeTypeCode);

        var history = new UserSubscriptionHistory
        {
            HistoryId = Guid.NewGuid(),
            UserSubscriptionId = existingSub.UserSubscriptionId,
            OldPlanId = oldPlanId,
            NewPlanId = plan.PlanId,
            OldPlanPriceId = oldPlanPriceId,
            NewPlanPriceId = price.PlanPriceId,
            ChangeTypeId = changeType.MasterValueId,
            EffectiveFrom = DateTime.UtcNow,
            ChangedAt = DateTime.UtcNow
        };
        await _billingRepository.AddSubscriptionHistoryAsync(history);
    }

    private async Task<long> GetPaymentStatusId(string code)
    {
        var statuses = await _billingRepository.GetMasterValuesByTypeAsync(BillingConstants.MasterTypes.PaymentStatus);
        return statuses.First(s => s.Code == code).MasterValueId;
    }
}
