using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeInOneMinute.Domain.Model;

[Table("subscription_plan_prices", Schema = "billing")]
public class SubscriptionPlanPrice
{
    [Key]
    public long PlanPriceId { get; set; }

    [Required]
    public long PlanId { get; set; }

    [Required]
    public long BillingCycleId { get; set; }

    [Required]
    public long CurrencyId { get; set; }

    [Required]
    [Column(TypeName = "numeric(10,2)")]
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("PlanId")]
    public virtual SubscriptionPlan? Plan { get; set; }

    [ForeignKey("BillingCycleId")]
    public virtual MasterValue? BillingCycle { get; set; }

    [ForeignKey("CurrencyId")]
    public virtual MasterValue? Currency { get; set; }
}
