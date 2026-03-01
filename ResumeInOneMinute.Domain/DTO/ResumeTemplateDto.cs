namespace ResumeInOneMinute.Domain.DTO;

/// <summary>
/// Full template response — returned from GET /api/templates/{id}
/// </summary>
public class ResumeTemplateDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LayoutType { get; set; } = string.Empty;
    public List<string> SectionOrder { get; set; } = new();
    public Dictionary<string, object> Theme { get; set; } = new();
    public Dictionary<string, object> Decorations { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Lightweight template item — returned in GET /api/templates list
/// </summary>
public class ResumeTemplateListDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LayoutType { get; set; } = string.Empty;
    public List<string> SectionOrder { get; set; } = new();
    public Dictionary<string, object> Theme { get; set; } = new();
    public Dictionary<string, object> Decorations { get; set; } = new();
}

/// <summary>
/// Request body for creating a new template (admin/seed use)
/// </summary>
public class CreateResumeTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string LayoutType { get; set; } = "single-column";
    public List<string> SectionOrder { get; set; } = new();
    public Dictionary<string, object> Theme { get; set; } = new();
    public Dictionary<string, object> Decorations { get; set; } = new();
}
