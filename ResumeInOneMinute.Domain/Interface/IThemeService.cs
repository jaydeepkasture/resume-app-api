using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Domain.Interface;

public interface IThemeService
{
    Task<ThemePagedResultDto<ResumeTheme>> GetThemesAsync(int page, int pageSize, string layoutType);
    Task<ResumeTheme> AddThemeAsync(ResumeThemeDto themeDto);
    Task<IEnumerable<ResumeTheme>> AddThemesAsync(IEnumerable<ResumeThemeDto> themeDtos);
    Task<bool> UpdateThemeAsync(string id, ResumeThemeDto themeDto);
    Task<bool> DeleteThemeAsync(string id);
    Task<ResumeTheme?> GetThemeByIdAsync(string id);
}
