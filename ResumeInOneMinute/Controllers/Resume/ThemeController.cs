using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Controllers.Resume;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/themes")]
[ApiController]
[Produces("application/json")]
public class ThemeController : ControllerBase
{
    private readonly IThemeService _themeService;

    public ThemeController(IThemeService themeService)
    {
        _themeService = themeService;
    }

    /// <summary>
    /// Get paginated list of active themes
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="layoutType">Layout type (e.g. single-column)</param>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ThemePagedResultDto<ResumeTheme>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetThemes([FromQuery] int page = 1, [FromQuery] int pageSize = 5, [FromQuery] string layoutType = "single-column")
    {
        page = Math.Max(1, page);
        pageSize = Math.Max(1, pageSize);

        var result = await _themeService.GetThemesAsync(page, pageSize, layoutType);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific theme by ID
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ResumeTheme), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetThemeById(string id)
    {
        var theme = await _themeService.GetThemeByIdAsync(id);
        if (theme == null) return NotFound();
        return Ok(theme);
    }

    /// <summary>
    /// Add a new theme
    /// </summary>
    [HttpPost]
    [Authorize] // Assuming only authorized users (admin) can add themes
    [ProducesResponseType(typeof(ResumeTheme), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTheme([FromBody] ResumeThemeDto themeDto)
    {
        var result = await _themeService.AddThemeAsync(themeDto);
        return CreatedAtAction(nameof(GetThemeById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Bulk add new themes
    /// </summary>
    [HttpPost("bulk")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<ResumeTheme>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateThemes([FromBody] IEnumerable<ResumeThemeDto> themeDtos)
    {
        var result = await _themeService.AddThemesAsync(themeDtos);
        return CreatedAtAction(nameof(GetThemes), null, result);
    }

    /// <summary>
    /// Update an existing theme
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTheme(string id, [FromBody] ResumeThemeDto themeDto)
    {
        var updated = await _themeService.UpdateThemeAsync(id, themeDto);
        if (!updated) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Delete a theme
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTheme(string id)
    {
        var deleted = await _themeService.DeleteThemeAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
