using System.Threading.Tasks;
using ResumeInOneMinute.Domain.DTO;

namespace ResumeInOneMinute.Domain.Interface;

public interface IPdfService
{
    Task<PdfGenerationResponseDto> GenerateResumePdfAsync(PdfGenerationRequestDto request);
}
