using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;
using ResumeInOneMinute.Controllers.Super;

namespace ResumeInOneMinute.Controllers.Layout;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/layouts")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class LayoutController : SuperController
{
    private readonly ILayoutRepository _layoutRepository;

    public LayoutController(ILayoutRepository layoutRepository)
    {
        _layoutRepository = layoutRepository;
    }

    /// <summary>
    /// Fetch a paginated list of layouts
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResponseList<LayoutDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLayouts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _layoutRepository.GetLayoutsAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Fetch the Layout configuration by its custom string ID
    /// </summary>
    [HttpGet("{layoutId}")]
    [ProducesResponseType(typeof(Response<LayoutDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<LayoutDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLayoutById(string layoutId)
    {
        var result = await _layoutRepository.GetLayoutByIdAsync(layoutId);
        return result.Status ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Insert a new layout configuration
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Response<LayoutDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<LayoutDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLayout([FromBody] LayoutDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new Response<LayoutDto>
            {
                Status = false,
                Message = "Validation failed",
                Data = default!
            });
        }

        var result = await _layoutRepository.CreateLayoutAsync(request);
        return result.Status ? Ok(result) : BadRequest(result);
    }
}
