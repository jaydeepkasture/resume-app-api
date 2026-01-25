using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ResumeInOneMinute.Domain.Model;

/// <summary>
/// Represents a single enhancement interaction in resume history.
/// Each entry contains: user message, AI response, original resume, enhanced resume, and HTML.
/// </summary>
[BsonIgnoreExtraElements]
public class ResumeEnhancementHistory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("user_id")]
    public long UserId { get; set; }
    
    /// <summary>
    /// Optional: Links this history entry to a chat session
    /// </summary>
    [BsonElement("chat_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ChatId { get; set; }
    
    /// <summary>
    /// User's message/instruction for enhancement (primary field)
    /// </summary>
    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// AI assistant's response message
    /// </summary>
    [BsonElement("assistant_message")]
    public string AssistantMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// Original resume JSON (before enhancement)
    /// </summary>
    [BsonElement("original_resume")]
    public BsonDocument? OriginalResume { get; set; }
    
    /// <summary>
    /// Enhanced resume JSON (after AI enhancement)
    /// </summary>
    [BsonElement("enhanced_resume")]
    public BsonDocument? EnhancedResume { get; set; }
    
    /// <summary>
    /// Resume HTML provided by user
    /// </summary>
    [BsonElement("resume_html")]
    public string? ResumeHtml { get; set; }
    
    /// <summary>
    /// Enhanced resume HTML returned by AI
    /// </summary>
    [BsonElement("enhanced_html")]
    public string? EnhancedHtml { get; set; }
    
    [BsonElement("template_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? TemplateId { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("processing_time_ms")]
    public long? ProcessingTimeMs { get; set; }
    
    // Legacy fields - kept ONLY for backward compatibility when reading old data
    // These will NOT be written to new MongoDB documents (no BsonElement attribute)
    [BsonIgnoreIfNull]
    public string? UserMessage { get; set; }
    
    [BsonIgnoreIfNull]
    public string? Role { get; set; }
    
    [BsonIgnoreIfNull]
    public string? EnhancementInstruction { get; set; }
}
