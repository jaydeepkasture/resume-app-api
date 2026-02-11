namespace ResumeInOneMinute.Domain.DTO;

public class SubscriptionPlanDto
{
    public long PlanId { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<SubscriptionPriceDto> Prices { get; set; } = new();
    public Dictionary<string, string> Benefits { get; set; } = new();
}

public class SubscriptionPriceDto
{
    public long PlanPriceId { get; set; }
    public string BillingCycle { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class UserSubscriptionDto
{
    public string PlanName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive => Status == "ACTIVE" && EndDate > DateTime.UtcNow;
    public Dictionary<string, string> Benefits { get; set; } = new();
}

public class CreateOrderRequestDto
{
    public string PlanPriceId { get; set; } = string.Empty;
    public string BillingCycle { get; set; } = "MONTHLY";
}

public class RazorpayOrderResponseDto
{
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class ConfirmPaymentRequestDto
{
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpaySignature { get; set; } = string.Empty;
}

public class SubscriptionConfirmResponseDto
{
    public string Status { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public Dictionary<string, string> Benefits { get; set; } = new();
}
