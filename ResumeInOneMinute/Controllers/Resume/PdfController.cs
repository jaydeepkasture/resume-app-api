using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Controllers.Resume;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/pdf")]
[ApiController]
[Produces("application/json")]
public class PdfController : ControllerBase
{
    private readonly IPdfService _pdfService;

    public PdfController(IPdfService pdfService)
    {
        _pdfService = pdfService;
    }

    /// <summary>
    /// Generate a PDF resume from theme and resume data
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(Response<PdfGenerationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GeneratePdf([FromBody] PdfGenerationRequestDto request)
    {
        if (request == null || string.IsNullOrEmpty(request.TemplateId) || request.Resume == null)
        {
            return BadRequest(new Response<object> 
            { 
                Status = false, 
                Message = "Invalid request. TemplateId and Resume data are required." 
            });
        }

        try
        {
            var result = await _pdfService.GenerateResumePdfAsync(request);
            return Ok(new Response<PdfGenerationResponseDto>
            {
                Status = true,
                Message = "PDF generated successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Template not found"))
            {
                return NotFound(new Response<object> 
                { 
                    Status = false, 
                    Message = ex.Message 
                });
            }
            
            return BadRequest(new Response<object> 
            { 
                Status = false, 
                Message = $"PDF generation failed: {ex.Message}" 
            });
        }
    }
}
