using ResumeInOneMinute.Domain.DTO;

namespace ResumeInOneMinute.Domain.Interface;

public interface IOllamaService
{
    Task<ResumeDto> EnhanceResumeAsync(ResumeDto originalResume, string enhancementInstruction);
    
    /// <summary>
    /// Enhance resume HTML from TiptapAngular editor
    /// </summary>
    /// <param name="resumeHtml">HTML content from TiptapAngular editor</param>
    /// <param name="resumeData">JSON resume data extracted from HTML or provided separately</param>
    /// <param name="enhancementMessage">User's enhancement instruction</param>
    /// <returns>Tuple containing enhanced HTML and enhanced resume data</returns>
    Task<(string EnhancedHtml, ResumeDto EnhancedResume)> EnhanceResumeHtmlAsync(
        string resumeHtml, 
        ResumeDto resumeData, 
        string enhancementMessage);

    /// <summary>
    /// Generate a chat title based on the user's first instruction
    /// </summary>
    /// <param name="instruction">User's first message/instruction</param>
    /// <returns>A short, summarized title</returns>
    Task<string> GenerateChatTitleAsync(string instruction);
}
