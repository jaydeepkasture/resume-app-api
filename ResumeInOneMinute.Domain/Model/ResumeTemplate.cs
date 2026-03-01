using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeInOneMinute.Domain.Model;

/// <summary>
/// Represents a resume rendering template.
/// Stores layout, theme, and decoration configuration as JSON/JSONB.
/// Backend only serves this data — frontend handles all rendering.
/// </summary>
[Table("resume_templates", Schema = "resume")]
public class ResumeTemplate
{
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Human-readable name (e.g., "Classic Teal", "Modern Dark")
    /// </summary>
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Layout variant: "single-column", "two-column", "sidebar"
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string LayoutType { get; set; } = "single-column";

    /// <summary>
    /// Ordered list of section keys the template renders
    /// (e.g., ["summary","experience","education","skills"])
    /// Stored as JSON in PostgreSQL.
    /// </summary>
    [Column(TypeName = "jsonb")]
    public List<string> SectionOrder { get; set; } = new();

    /// <summary>
    /// Theme configuration (colors, fonts, spacing, etc.)
    /// Stored as JSONB in PostgreSQL.
    /// Example: { "primaryColor": "#008080", "fontFamily": "Inter", ... }
    /// </summary>
    [Column(TypeName = "jsonb")]
    public Dictionary<string, object> Theme { get; set; } = new();

    /// <summary>
    /// Visual decorations (borders, dividers, icons, background patterns, etc.)
    /// Stored as JSONB in PostgreSQL.
    /// Example: { "headerBorder": true, "sectionDivider": "line", ... }
    /// </summary>
    [Column(TypeName = "jsonb")]
    public Dictionary<string, object> Decorations { get; set; } = new();

    /// <summary>
    /// Soft-delete / visibility flag. Only active templates are served to users.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; } = null;
}
