using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using System.Security.Claims;

namespace ResumeInOneMinute.Controllers.Resume;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/resume")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class ResumeController : SuperController
{
    private readonly IResumeRepository _resumeRepository;
    private readonly IAccountRepository _accountRepository;

    public ResumeController(IResumeRepository resumeRepository, IAccountRepository accountRepository)
    {
        _resumeRepository = resumeRepository;
        _accountRepository = accountRepository;
    }

    #region Chat-Based Enhancement (New - ChatGPT-like)

    /// <summary>
    /// Create a new chat session with dummy resume data for the user
    /// </summary>
    [HttpPost("chat/create")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateChatSession()
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized(new { Status = false, Message = "User not authenticated" });
        }

        // Fetch user details for dummy data
        var user = await _accountRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return Unauthorized(new { Status = false, Message = "User not found" });
        }

        // Create dummy resume data
        var dummyResume = new ResumeDto
        {
            Name = $"{user.UserProfile?.FirstName} {user.UserProfile?.LastName}".Trim(),
            Email = user.Email,
            PhoneNo = user.UserProfile?.Phone ?? string.Empty,
            Location = "City, Country",
            LinkedIn = "linkedin.com/in/username",
            GitHub = "github.com/username",
            Summary = "Experienced professional with a proven track record...",
            Experience = new List<ExperienceDto>
            {
                new ExperienceDto
                {
                    Company = "Tech Corp",
                    Position = "Software Engineer",
                    From = "2020",
                    To = "Present",
                    Description = "Developed and maintained web applications..."
                }
            },
            Education = new List<EducationDto>
            {
                new EducationDto
                {
                    Institution = "University of Technology",
                    Degree = "Bachelor of Science",
                    Field = "Computer Science",
                    Year = "2019"
                }
            },
            Skills = new List<string> { "C#", ".NET", "SQL", "JavaScript" }
        };

        var request = new CreateChatSessionDto
        {
            Title = "New Resume Chat",
            InitialResume = dummyResume
        };

        var result = await _resumeRepository.CreateChatSessionAsync(userId, request);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Send a message in a chat session (with optional resume data)
    /// </summary>
    /// <param name="request">Chat message with optional chatId and resume data</param>
    [HttpPost("chat/enhance")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChatEnhance([FromBody] ChatEnhancementRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                Status = false,
                Message = "Validation failed",
                Data = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
            });
        }

        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized(new { Status = false, Message = "User not authenticated" });
        }

        var result = await _resumeRepository.ChatEnhanceAsync(userId, request);
        return result.Status ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get all chat sessions for the current user
    /// </summary>
    [HttpGet("chat/sessions")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChatSessions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized(new { Status = false, Message = "User not authenticated" });
        }

        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var result = await _resumeRepository.GetUserChatSessionsAsync(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific chat session with all messages
    /// </summary>
    [HttpGet("chat/{chatId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChatSession(string chatId)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized(new { Status = false, Message = "User not authenticated" });
        }

        var result = await _resumeRepository.GetChatSessionByIdAsync(userId, chatId);
        return result.Status ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Get paginated enhancement history summary for a specific chat
    /// </summary>
    [HttpGet("chat/{chatId}/history")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChatHistorySummary(string chatId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string sortOrder = "desc", [FromQuery] string search = "", [FromQuery] string? templateId = null)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized(new { Status = false, Message = "User not authenticated" });
        }

        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var result = await _resumeRepository.GetChatHistorySummaryAsync(userId, chatId, page, pageSize, sortOrder, search, templateId);
        return Ok(result);
    }

    /// <summary>
    /// Get detailed enhancement history record by ID
    /// </summary>
    [HttpGet("chat/history/{historyId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChatHistoryDetail(string historyId)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized(new { Status = false, Message = "User not authenticated" });
        }

        var result = await _resumeRepository.GetEnhancementHistoryDetailAsync(userId, historyId);
        return result.Status ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Delete a chat session
    /// </summary>
    [HttpDelete("chat/{chatId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteChatSession(string chatId)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized(new { Status = false, Message = "User not authenticated" });
        }

        var result = await _resumeRepository.DeleteChatSessionAsync(userId, chatId);
        return result.Status ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Update chat session title
    /// </summary>
    [HttpPatch("chat/{chatId}/title")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateChatTitle(string chatId, [FromBody] UpdateChatTitleDto request)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized(new { Status = false, Message = "User not authenticated" });
        }

        var result = await _resumeRepository.UpdateChatTitleAsync(userId, chatId, request.Title);
        return result.Status ? Ok(result) : NotFound(result);
    }

    #endregion

    #region Legacy Enhancement (Backward Compatibility)

    /// <summary>
    /// Enhance a resume using AI (Ollama) - Legacy endpoint
    /// </summary>
    /// <param name="request">Resume data and enhancement instructions</param>
    /// <returns>Enhanced resume with history ID</returns>
    [HttpPost("enhance")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EnhanceResume([FromBody] ResumeEnhancementRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new 
            { 
                Status = false, 
                Message = "Validation failed", 
                Data = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) 
            });
        }

        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized(new { Status = false, Message = "User not authenticated" });
        }

        var result = await _resumeRepository.EnhanceResumeAsync(userId, request);

        if (!result.Status)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get user's resume enhancement history - Legacy endpoint
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 50)</param>
    /// <returns>List of enhancement history records</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized(new { Status = false, Message = "User not authenticated" });
        }

        // Limit page size to prevent abuse
        pageSize = Math.Min(pageSize, 50);
        page = Math.Max(page, 1);

        var result = await _resumeRepository.GetUserHistoryAsync(userId, page, pageSize);

        if (!result.Status)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get a specific enhancement history record by ID - Legacy endpoint
    /// </summary>
    /// <param name="historyId">MongoDB ObjectId of the history record</param>
    /// <returns>Enhancement history record</returns>
    [HttpGet("history/{historyId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistoryById(string historyId)
    {
        var userId = GetUserId();
        if (userId == 0)
        {
            return Unauthorized(new { Status = false, Message = "User not authenticated" });
        }

        if (string.IsNullOrWhiteSpace(historyId))
        {
            return BadRequest(new { Status = false, Message = "History ID is required" });
        }

        var result = await _resumeRepository.GetHistoryByIdAsync(userId, historyId);

        if (!result.Status)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    #endregion

    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}

/// <summary>
/// DTO for updating chat title
/// </summary>
public class UpdateChatTitleDto
{
    public string Title { get; set; } = string.Empty;
}
