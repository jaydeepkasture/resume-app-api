using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeInOneMinute.Domain.Model;

[Table("subscription_payments", Schema = "billing")]
public class SubscriptionPayment
{
    [Key]
    public Guid PaymentId { get; set; } = Guid.NewGuid();

    public Guid? UserSubscriptionId { get; set; }

    [Required]
    public long PaymentProviderId { get; set; }

    [Required]
    public long PaymentStatusId { get; set; }

    [Required]
    public long PlanPriceId { get; set; }

    [Required]
    [Column(TypeName = "numeric(10,2)")]
    public decimal Amount { get; set; }

    [Required]
    public long CurrencyId { get; set; }

    [MaxLength(100)]
    public string? ProviderPaymentId { get; set; }

    [MaxLength(100)]
    public string? ProviderOrderId { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserSubscriptionId")]
    public virtual UserSubscription? UserSubscription { get; set; }

    [ForeignKey("PlanPriceId")]
    public virtual SubscriptionPlanPrice? PlanPrice { get; set; }

    [ForeignKey("PaymentProviderId")]
    public virtual MasterValue? PaymentProvider { get; set; }

    [ForeignKey("PaymentStatusId")]
    public virtual MasterValue? PaymentStatus { get; set; }

    [ForeignKey("CurrencyId")]
    public virtual MasterValue? Currency { get; set; }
}
