using System.ComponentModel.DataAnnotations;

namespace ResumeInOneMinute.Domain.DTO;

public class ResumeEnhancementRequestDto
{
    [Required]
    public ResumeDto ResumeData { get; set; } = new();
    
    [Required]
    [StringLength(100000, MinimumLength = 10)]
    public string EnhancementInstruction { get; set; } = string.Empty;
}
