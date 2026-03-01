using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ResumeInOneMinute.Domain.Model;

public class Layout
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("layout_id")]
    [BsonRequired]
    public string LayoutId { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("styles")]
    public Dictionary<string, string> Styles { get; set; } = new();

    [BsonElement("header")]
    public LayoutHeader Header { get; set; } = new();

    [BsonElement("sections")]
    public List<LayoutSection> Sections { get; set; } = new();

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class LayoutHeader
{
    [BsonElement("alignment")]
    public string Alignment { get; set; } = "center";

    [BsonElement("name")]
    public LayoutHeaderName Name { get; set; } = new();

    [BsonElement("contact")]
    public LayoutHeaderContact Contact { get; set; } = new();

    [BsonElement("divider")]
    public LayoutHeaderDivider Divider { get; set; } = new();
}

public class LayoutHeaderName
{
    [BsonElement("visible")]
    public bool Visible { get; set; } = true;

    [BsonElement("size")]
    public string Size { get; set; } = "36px";

    [BsonElement("weight")]
    public string Weight { get; set; } = "700";
}

public class LayoutHeaderContact
{
    [BsonElement("visible")]
    public bool Visible { get; set; } = true;

    [BsonElement("size")]
    public string Size { get; set; } = "14px";

    [BsonElement("separator")]
    public string Separator { get; set; } = "|";

    [BsonElement("visible_fields")]
    public List<string> VisibleFields { get; set; } = new();
}

public class LayoutHeaderDivider
{
    [BsonElement("enabled")]
    public bool Enabled { get; set; } = true;

    [BsonElement("color")]
    public string Color { get; set; } = "#e0e0e0";

    [BsonElement("height")]
    public string Height { get; set; } = "2px";

    [BsonElement("margin_top")]
    public string MarginTop { get; set; } = "8px";

    [BsonElement("margin_bottom")]
    public string MarginBottom { get; set; } = "24px";

    [BsonElement("width")]
    public string Width { get; set; } = "100%";
}

public class LayoutSection
{
    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;

    [BsonElement("enabled")]
    public bool Enabled { get; set; } = true;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;
}
