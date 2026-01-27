using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;

using System.Security.Claims;

namespace ResumeInOneMinute.Controllers.Template;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/template")]
[Authorize]
public class HtmlTemplateController : ControllerBase
{
    private readonly IHtmlTemplateRepository _templateRepository;
    private readonly ILogger<HtmlTemplateController> _logger;

    public HtmlTemplateController(
        IHtmlTemplateRepository templateRepository,
        ILogger<HtmlTemplateController> logger)
    {
        _templateRepository = templateRepository;
        _logger = logger;
    }

    /// <summary>
    /// Create a new HTML template
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateHtmlTemplateDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.HtmlTemplate))
            {
                return BadRequest(new Response<object>
                {
                    Status = false,
                    Message = "HTML template content is required"
                });
            }

            if (string.IsNullOrWhiteSpace(dto.TemplateName))
            {
                return BadRequest(new Response<object>
                {
                    Status = false,
                    Message = "Template name is required"
                });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var template = await _templateRepository.CreateTemplateAsync(dto, userId);

            return Ok(new Response<HtmlTemplateResponseDto>
            {
                Status = true,
                Message = "HTML template created successfully",
                Data = template
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating HTML template");
            return StatusCode(500, new Response<object>
            {
                Status = false,
                Message = "An error occurred while creating the template"
            });
        }
    }

    /// <summary>
    /// Get paginated list of HTML templates
    /// </summary>
    [HttpGet("list")]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? templateTypeId = null)
    {
        try
        {
            if (page < 1)
            {
                return BadRequest(new Response<object>
                {
                    Status = false,
                    Message = "Page number must be greater than 0"
                });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new Response<object>
                {
                    Status = false,
                    Message = "Page size must be between 1 and 100"
                });
            }

            var result = await _templateRepository.GetTemplatesAsync(page, pageSize, search, templateTypeId);

            return Ok(new Response<PaginatedHtmlTemplatesDto>
            {
                Status = true,
                Message = "Templates retrieved successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HTML templates");
            return StatusCode(500, new Response<object>
            {
                Status = false,
                Message = "An error occurred while retrieving templates"
            });
        }
    }

    /// <summary>
    /// Get HTML template by ID
    /// </summary>
    [HttpGet("{templateId}")]
    public async Task<IActionResult> GetTemplateById(string templateId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(templateId))
            {
                return BadRequest(new Response<object>
                {
                    Status = false,
                    Message = "Template ID is required"
                });
            }

            var template = await _templateRepository.GetTemplateByIdAsync(templateId);

            if (template == null)
            {
                return NotFound(new Response<object>
                {
                    Status = false,
                    Message = "Template not found"
                });
            }

            return Ok(new Response<HtmlTemplateResponseDto>
            {
                Status = true,
                Message = "Template retrieved successfully",
                Data = template
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HTML template: {TemplateId}", templateId);
            return StatusCode(500, new Response<object>
            {
                Status = false,
                Message = "An error occurred while retrieving the template"
            });
        }
    }

    /// <summary>
    /// Delete HTML template by ID
    /// </summary>
    [HttpDelete("{templateId}")]
    public async Task<IActionResult> DeleteTemplate(string templateId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(templateId))
            {
                return BadRequest(new Response<object>
                {
                    Status = false,
                    Message = "Template ID is required"
                });
            }

            var deleted = await _templateRepository.DeleteTemplateAsync(templateId);

            if (!deleted)
            {
                return NotFound(new Response<object>
                {
                    Status = false,
                    Message = "Template not found"
                });
            }

            return Ok(new Response<object>
            {
                Status = true,
                Message = "Template deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting HTML template: {TemplateId}", templateId);
            return StatusCode(500, new Response<object>
            {
                Status = false,
                Message = "An error occurred while deleting the template"
            });
        }
    }
}

