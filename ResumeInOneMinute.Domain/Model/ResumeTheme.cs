using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ResumeInOneMinute.Domain.Model;

public class ResumeTheme
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("layoutType")]
    public string LayoutType { get; set; } = "single-column";

    [BsonElement("previewImage")]
    public string PreviewImage { get; set; } = string.Empty;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("theme")]
    public ThemeTokens Theme { get; set; } = new();

    [BsonElement("decorations")]
    public ThemeDecorations Decorations { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ThemeTokens
{
    [BsonElement("colors")]
    public ThemeColors Colors { get; set; } = new();

    [BsonElement("typography")]
    public ThemeTypography Typography { get; set; } = new();

    [BsonElement("spacing")]
    public ThemeSpacing Spacing { get; set; } = new();
}

public class ThemeColors
{
    [BsonElement("primary")]
    public string Primary { get; set; } = string.Empty;

    [BsonElement("textPrimary")]
    public string TextPrimary { get; set; } = string.Empty;

    [BsonElement("background")]
    public string Background { get; set; } = string.Empty;
}

public class ThemeTypography
{
    [BsonElement("fontFamily")]
    public string FontFamily { get; set; } = string.Empty;

    [BsonElement("name")]
    public FontStyle Name { get; set; } = new();

    [BsonElement("contact")]
    public FontStyle Contact { get; set; } = new();

    [BsonElement("sectionTitle")]
    public FontStyle SectionTitle { get; set; } = new();

    [BsonElement("body")]
    public FontStyle Body { get; set; } = new();
}

public class FontStyle
{
    [BsonElement("size")]
    public string Size { get; set; } = string.Empty;

    [BsonElement("weight")]
    [BsonIgnoreIfNull]
    public string? Weight { get; set; }

    [BsonElement("lineHeight")]
    [BsonIgnoreIfNull]
    public string? LineHeight { get; set; }
}

public class ThemeSpacing
{
    [BsonElement("headerBottom")]
    public string HeaderBottom { get; set; } = string.Empty;

    [BsonElement("sectionGap")]
    public string SectionGap { get; set; } = string.Empty;

    [BsonElement("paragraphGap")]
    public string ParagraphGap { get; set; } = string.Empty;
}

public class ThemeDecorations
{
    [BsonElement("headerDivider")]
    public HeaderDivider HeaderDivider { get; set; } = new();
}

public class HeaderDivider
{
    [BsonElement("enabled")]
    public bool Enabled { get; set; }

    [BsonElement("height")]
    public string Height { get; set; } = "1px";

    [BsonElement("color")]
    public string Color { get; set; } = string.Empty;

    [BsonElement("marginTop")]
    public string MarginTop { get; set; } = "0px";

    [BsonElement("marginBottom")]
    public string MarginBottom { get; set; } = "0px";
}
