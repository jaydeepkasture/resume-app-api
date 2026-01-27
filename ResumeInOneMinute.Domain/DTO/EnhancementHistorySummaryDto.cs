namespace ResumeInOneMinute.Domain.DTO;

public class EnhancementHistorySummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string UserMessage { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    public DateTime CreatedAt { get; set; }
    public ResumeDto? ResumeData { get; set; }
}
