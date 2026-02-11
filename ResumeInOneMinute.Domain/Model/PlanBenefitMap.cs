using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeInOneMinute.Domain.Model;

[Table("plan_benefit_map", Schema = "billing")]
public class PlanBenefitMap
{
    [Key]
    [Column("map_id")]
    public long MapId { get; set; }

    [Column("plan_id")]
    public long PlanId { get; set; }

    [Column("benefit_id")]
    public long BenefitId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("benefit_value")]
    public string BenefitValue { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("PlanId")]
    public virtual SubscriptionPlan? Plan { get; set; }

    [ForeignKey("BenefitId")]
    public virtual PlanBenefit? Benefit { get; set; }
}
