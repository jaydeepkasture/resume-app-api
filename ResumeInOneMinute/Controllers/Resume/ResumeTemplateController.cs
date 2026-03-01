using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;
using ResumeInOneMinute.Controllers.Super;

namespace ResumeInOneMinute.Controllers.Resume;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/templates")]
[Authorize]
public class ResumeTemplateController : SuperController
{
    private readonly IResumeTemplateRepository _templateRepository;
    private readonly ILogger<ResumeTemplateController> _logger;

    public ResumeTemplateController(
        IResumeTemplateRepository templateRepository,
        ILogger<ResumeTemplateController> logger)
    {
        _templateRepository = templateRepository;
        _logger = logger;
    }

    /// <summary>
    /// Returns a specific template by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Response<ResumeTemplateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTemplateById(long id)
    {
        try
        {
            var response = await _templateRepository.GetByIdAsync(id);
            if (!response.Status)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching template {Id}", id);
            return StatusCode(500, new Response<object> { Status = false, Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Returns a list of all active templates
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Response<List<ResumeTemplateListDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveTemplates()
    {
        try
        {
            var response = await _templateRepository.GetActiveTemplatesAsync();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active templates");
            return StatusCode(500, new Response<object> { Status = false, Message = "Internal server error" });
        }
    }

    /// <summary>
    /// Admin-only: Create a new resume template
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")] // Example: Restricting creation to admins
    public async Task<IActionResult> CreateTemplate([FromBody] CreateResumeTemplateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new Response<object> { Status = false, Message = "Invalid data" });
        }

        try
        {
            var response = await _templateRepository.CreateAsync(dto);
            return CreatedAtAction(nameof(GetTemplateById), new { id = response.Data.Id, version = "1.0" }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            return StatusCode(500, new Response<object> { Status = false, Message = "Internal server error" });
        }
    }
}
