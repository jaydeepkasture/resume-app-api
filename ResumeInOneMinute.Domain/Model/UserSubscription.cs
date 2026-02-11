using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeInOneMinute.Domain.Model;

[Table("user_subscriptions", Schema = "billing")]
public class UserSubscription
{
    [Key]
    public Guid UserSubscriptionId { get; set; } = Guid.NewGuid();

    [Required]
    public long UserId { get; set; }

    [Required]
    public Guid GlobalUserId { get; set; }

    [Required]
    public long PlanId { get; set; }

    [Required]
    public long PlanPriceId { get; set; }

    [Required]
    public long StatusId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public bool AutoRenew { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    [ForeignKey("PlanId")]
    public virtual SubscriptionPlan? Plan { get; set; }

    [ForeignKey("PlanPriceId")]
    public virtual SubscriptionPlanPrice? PlanPrice { get; set; }

    [ForeignKey("StatusId")]
    public virtual MasterValue? Status { get; set; }
}
