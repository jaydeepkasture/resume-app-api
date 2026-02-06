using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ResumeInOneMinute.Domain.Model;

[BsonIgnoreExtraElements]
public class HtmlTemplate
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("html_template")]
    public string HtmlTemplateContent { get; set; } = string.Empty;

    [BsonElement("template_name")]
    public string TemplateName { get; set; } = string.Empty;

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("created_by")]
    public string CreatedBy { get; set; } = string.Empty;

    [BsonElement("template_type_id ")]
    public int? TemplateTypeId { get; set; }

    [BsonElement("is_active")]
    public bool IsActive { get; set; } = true;
}
