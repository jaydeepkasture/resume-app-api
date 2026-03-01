using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Domain.Interface;

public interface IResumeTemplateRepository
{
    /// <summary>
    /// Get a single template by ID
    /// </summary>
    Task<Response<ResumeTemplateDto>> GetByIdAsync(long id);

    /// <summary>
    /// Get all active templates (IsActive = true)
    /// </summary>
    Task<Response<List<ResumeTemplateListDto>>> GetActiveTemplatesAsync();

    /// <summary>
    /// Create a new template (admin/seed)
    /// </summary>
    Task<Response<ResumeTemplateDto>> CreateAsync(CreateResumeTemplateDto dto);
}
