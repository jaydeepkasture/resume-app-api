namespace ResumeInOneMinute.Domain.DTO;

public class EnhancementHistoryDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string ChatId { get; set; } = string.Empty;
    public string? TemplateId { get; set; }
    
    // Message info
    public string UserMessage { get; set; } = string.Empty;
    public string AssistantMessage { get; set; } = string.Empty;
    
    // Resume data
    public ResumeDto? OriginalResume { get; set; }
    public ResumeDto? EnhancedResume { get; set; }
    public string? ResumeHtml { get; set; }
    public string? EnhancedHtml { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public long? ProcessingTimeMs { get; set; }
}
