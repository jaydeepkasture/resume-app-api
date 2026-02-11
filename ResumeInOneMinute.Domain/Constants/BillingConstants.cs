namespace ResumeInOneMinute.Domain.Constants;

public static class BillingConstants
{
    public static class MasterTypes
    {
        public const string PlanCode = "PLAN_CODE";
        public const string BillingCycle = "BILLING_CYCLE";
        public const string Currency = "CURRENCY";
        public const string SubscriptionStatus = "SUBSCRIPTION_STATUS";
        public const string SubscriptionChangeType = "SUBSCRIPTION_CHANGE_TYPE";
        public const string PaymentStatus = "PAYMENT_STATUS";
        public const string PaymentProvider = "PAYMENT_PROVIDER";
    }

    public static class PlanCodes
    {
        public const string Free = "FREE";
        public const string Pro = "PRO";
        public const string Enterprise = "ENTERPRISE";
    }

    public static class BillingCycles
    {
        public const string Monthly = "MONTHLY";
        public const string Yearly = "YEARLY";
    }

    public static class Currencies
    {
        public const string Inr = "INR";
        public const string Usd = "USD";
    }

    public static class SubscriptionStatuses
    {
        public const string Active = "ACTIVE";
        public const string Cancelled = "CANCELLED";
        public const string Expired = "EXPIRED";
        public const string Paused = "PAUSED";
    }

    public static class SubscriptionChangeTypes
    {
        public const string Upgrade = "UPGRADE";
        public const string Downgrade = "DOWNGRADE";
        public const string Renewal = "RENEWAL";
        public const string Cancel = "CANCEL";
    }

    public static class PaymentStatuses
    {
        public const string Pending = "PENDING";
        public const string Success = "SUCCESS";
        public const string Failed = "FAILED";
    }

    public static class PaymentProviders
    {
        public const string Razorpay = "RAZORPAY";
    }
}
