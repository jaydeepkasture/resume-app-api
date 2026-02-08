using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ResumeInOneMinute.Domain.DTO;

namespace ResumeInOneMinute.Domain.Model;

/// <summary>
/// Represents a user's master resume data.
/// Only one entry per user exists in the 'resume' collection.
/// </summary>
public class Resume
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("user_id")]
    public long UserId { get; set; }

    [BsonElement("resume_data")]
    public ResumeDto ResumeData { get; set; } = new();

    /// <summary>
    /// Source file type or method of creation (e.g., "pdf", "docx", "image", "manual")
    /// </summary>
    [BsonElement("parsed_from")]
    public string ParsedFrom { get; set; } = string.Empty;

    [BsonElement("parsed_at")]
    public DateTime ParsedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
