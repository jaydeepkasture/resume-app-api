using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Domain.Interface;

public interface ILayoutRepository
{
    Task<ResponseList<LayoutDto>> GetLayoutsAsync(int page = 1, int pageSize = 10);
    Task<Response<LayoutDto>> GetLayoutByIdAsync(string layoutId);
    Task<Response<LayoutDto>> CreateLayoutAsync(LayoutDto dto);
    Task SeedDefaultLayoutAsync();
}
