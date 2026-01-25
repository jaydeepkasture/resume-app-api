using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Domain.Interface;

public interface IHtmlTemplateRepository
{
    Task<HtmlTemplateResponseDto> CreateTemplateAsync(CreateHtmlTemplateDto dto, string userId);
    Task<PaginatedHtmlTemplatesDto> GetTemplatesAsync(int page = 1, int pageSize = 10, string? search = null);
    Task<HtmlTemplateResponseDto?> GetTemplateByIdAsync(string templateId);
    Task<bool> DeleteTemplateAsync(string templateId);
}
