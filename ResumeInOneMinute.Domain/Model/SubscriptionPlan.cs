using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeInOneMinute.Domain.Model;

[Table("subscription_plans", Schema = "billing")]
public class SubscriptionPlan
{
    [Key]
    public long PlanId { get; set; }

    [Required]
    public long PlanCodeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string PlanName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("PlanCodeId")]
    public virtual MasterValue? PlanCode { get; set; }

    public virtual ICollection<PlanBenefitMap> Benefits { get; set; } = new List<PlanBenefitMap>();
}
