using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeInOneMinute.Domain.Model;

[Table("plan_benefits", Schema = "billing")]
public class PlanBenefit
{
    [Key]
    [Column("benefit_id")]
    public long BenefitId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("benefit_code")]
    public string BenefitCode { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("benefit_name")]
    public string BenefitName { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
