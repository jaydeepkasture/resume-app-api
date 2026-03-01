using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Domain.DTO;

public class ResumeThemeDto
{
    public string Name { get; set; } = string.Empty;
    public string LayoutType { get; set; } = "single-column";
    public string PreviewImage { get; set; } = string.Empty;
    public ThemeTokens Theme { get; set; } = new();
    public ThemeDecorations Decorations { get; set; } = new();
}
