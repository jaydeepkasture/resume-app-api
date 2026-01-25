namespace ResumeInOneMinute.Domain.DTO;

public class ResumeEnhancementResponseDto
{
    public ResumeDto OriginalResume { get; set; } = new();
    
    public ResumeDto EnhancedResume { get; set; } = new();
    
    public string EnhancementInstruction { get; set; } = string.Empty;
    
    public string HistoryId { get; set; } = string.Empty;
    
    public DateTime ProcessedAt { get; set; }

    public string? TemplateId { get; set; }
}
