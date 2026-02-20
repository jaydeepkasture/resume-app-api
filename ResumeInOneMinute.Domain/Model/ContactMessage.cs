using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ResumeInOneMinute.Domain.Model;

[BsonIgnoreExtraElements]
public class ContactMessage
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    [BsonElement("submitted_at")]
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("is_resolved")]
    public bool IsResolved { get; set; } = false;
}
