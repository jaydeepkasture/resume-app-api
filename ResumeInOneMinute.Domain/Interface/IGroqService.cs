using ResumeInOneMinute.Domain.DTO;

namespace ResumeInOneMinute.Domain.Interface;

public interface IGroqService
{
    Task<ResumeDto> ExtractResumeFromTextAsync(string text);
    Task<ResumeDto> ExtractResumeFromImageAsync(string base64Image);
}
