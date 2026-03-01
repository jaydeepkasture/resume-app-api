using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Domain.Interface;

public interface IResumeSqlRepository
{
    /// <summary>
    /// Get a resume from PostgreSQL storage by ID
    /// </summary>
    Task<Response<ResumeDto>> GetResumeByIdAsync(long userId);

    /// <summary>
    /// Create or Update a resume in PostgreSQL (JSONB storage)
    /// </summary>
    Task<Response<long>> SaveResumeAsync(long userId, long templateId, ResumeDto resumeData);
}
