using System.ComponentModel.DataAnnotations;


namespace ResumeInOneMinute.Domain.DTO;

/// <summary>
/// Request DTO for chat-based resume enhancement (like ChatGPT)
/// </summary>
public class ChatEnhancementRequestDto
{
    /// <summary>
    /// Optional chat ID. If null, creates a new chat session.
    /// </summary>
    public string? ChatId { get; set; }
    
    /// <summary>
    /// User's message/instruction for enhancement
    /// </summary>
    [Required]
    [StringLength(100000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional resume data. Can be partial or complete.
    /// If not provided, uses the resume from chat history.
    /// </summary>
    public ResumeDto? ResumeData { get; set; }
    
    /// <summary>
    /// Optional resume HTML provided by user
    /// </summary>
    public string? ResumeHtml { get; set; }

    /// <summary>
    /// Optional template ID if using a specific HTML template
    /// </summary>
    public string? TemplateId { get; set; }
}

/// <summary>
/// Response DTO for chat-based enhancement
/// </summary>
public class ChatEnhancementResponseDto
{
    /// <summary>
    /// Chat session ID
    /// </summary>
    public string ChatId { get; set; } = string.Empty;

    /// <summary>
    /// Updated Chat Title
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// User's message
    /// </summary>
    public string UserMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// AI assistant's response
    /// </summary>
    public string AssistantMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// Current resume state (if applicable)
    /// </summary>
    public ResumeDto? CurrentResume { get; set; }
    
    /// <summary>
    /// Enhanced HTML from TiptapAngular editor (if HTML was provided in request)
    /// </summary>
    public string? EnhancedHtml { get; set; }
    
    /// <summary>
    /// Processing time in milliseconds
    /// </summary>
    public long ProcessingTimeMs { get; set; }
    
    /// <summary>
    /// Timestamp of the response
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// DTO for creating a new chat session
/// </summary>
public class CreateChatSessionDto
{
    /// <summary>
    /// Template ID if creating from a template
    /// </summary>
    public string? TemplateId { get; set; }
}

/// <summary>
/// DTO for chat session summary
/// </summary>
public class ChatSessionSummaryDto
{
    public string ChatId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int MessageCount { get; set; }
    public bool IsActive { get; set; }
    public ResumeDto? ResumeData { get; set; }
}

/// <summary>
/// DTO for full chat session with messages
/// </summary>
public class ChatSessionDetailDto
{
    public string ChatId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ResumeDto? ResumeData { get; set; }
    public bool IsActive { get; set; }
    public string? TemplateId { get; set; }
}

/// <summary>
/// DTO for a single chat message
/// </summary>
public class ChatMessageDto
{
    public string Id { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
    public ResumeDto? ResumeData { get; set; }
    public DateTime Timestamp { get; set; }
    public long? ProcessingTimeMs { get; set; }
}
