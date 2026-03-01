using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ResumeInOneMinute.Domain.DTO;

namespace ResumeInOneMinute.Domain.Model;

/// <summary>
/// Represents a user's resume associated with a specific template.
/// Stored in PostgreSQL to leverage relational integrity and JSONB querying.
/// </summary>
[Table("user_resumes", Schema = "resume")]
public class UserResume
{
    [Key]
    public long Id { get; set; }

    [Required]
    public long UserId { get; set; }

    [Required]
    public long TemplateId { get; set; }

    /// <summary>
    /// The actual resume content (Experience, Education, etc.) stored as JSONB.
    /// Maps to ResumeDto.
    /// </summary>
    [Column(TypeName = "jsonb")]
    public ResumeDto ResumeData { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; } = null;

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("TemplateId")]
    public virtual ResumeTemplate Template { get; set; } = null!;
}
