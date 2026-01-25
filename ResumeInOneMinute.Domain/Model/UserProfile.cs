using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeInOneMinute.Domain.Model;

[Table("user_profiles", Schema = "auth")]
public class UserProfile
{
    [Key]
    public long UserProfileId { get; set; }

    [Required]
    public long UserId { get; set; }

    [Column("global_user_profile_id")]
    public Guid GlobalUserProfileId { get; set; }

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [MaxLength(10)]
    public string? Phone { get; set; }

    [MaxLength(3)]
    public string? CountryCode { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
