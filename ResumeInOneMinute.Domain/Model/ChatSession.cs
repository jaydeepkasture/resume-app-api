using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ResumeInOneMinute.Domain.Model;

/// <summary>
/// Represents a chat session metadata for resume enhancement conversations.
/// Actual messages and resume data are stored in ResumeEnhancementHistory collection.
/// </summary>
public class ChatSession
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    [BsonElement("user_id")]
    public long UserId { get; set; }
    
    [BsonElement("title")]
    public string Title { get; set; } = "New Chat";
    
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("template_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? TemplateId { get; set; }
    
    [BsonElement("is_active")]
    public bool IsActive { get; set; } = true;

    [BsonElement("is_deleted")]
    public bool IsDeleted { get; set; } = false;
}
