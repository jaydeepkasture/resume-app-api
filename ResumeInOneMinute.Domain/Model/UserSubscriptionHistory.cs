using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeInOneMinute.Domain.Model;

[Table("user_subscription_history", Schema = "billing")]
public class UserSubscriptionHistory
{
    [Key]
    public Guid HistoryId { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UserSubscriptionId { get; set; }

    public long? OldPlanId { get; set; }

    [Required]
    public long NewPlanId { get; set; }

    public long? OldPlanPriceId { get; set; }

    [Required]
    public long NewPlanPriceId { get; set; }

    [Required]
    public long ChangeTypeId { get; set; }

    [Required]
    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserSubscriptionId")]
    public virtual UserSubscription? UserSubscription { get; set; }

    [ForeignKey("ChangeTypeId")]
    public virtual MasterValue? ChangeType { get; set; }
}
