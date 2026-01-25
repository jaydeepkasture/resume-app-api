namespace ResumeInOneMinute.Domain.DTO;

public class CreateHtmlTemplateDto
{
    public string HtmlTemplate { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
}

public class HtmlTemplateResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string HtmlTemplate { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class HtmlTemplateListDto
{
    public string Id { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

public class PaginatedHtmlTemplatesDto
{
    public List<HtmlTemplateListDto> Templates { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
