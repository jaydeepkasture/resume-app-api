namespace ResumeInOneMinute.Domain.DTO;

public class LayoutDto
{
    public string LayoutId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, string> Styles { get; set; } = new();
    public LayoutHeaderDto Header { get; set; } = new();
    public List<LayoutSectionDto> Sections { get; set; } = new();
}

public class LayoutHeaderDto
{
    public string Alignment { get; set; } = "center";
    public HeaderNameDto Name { get; set; } = new();
    public HeaderContactDto Contact { get; set; } = new();
    public HeaderDividerDto Divider { get; set; } = new();
}

public class HeaderNameDto
{
    public bool Visible { get; set; } = true;
    public string Size { get; set; } = "36px";
    public string Weight { get; set; } = "700";
}

public class HeaderContactDto
{
    public bool Visible { get; set; } = true;
    public string Size { get; set; } = "14px";
    public string Separator { get; set; } = "|";
    public List<string> VisibleFields { get; set; } = new();
}

public class HeaderDividerDto
{
    public bool Enabled { get; set; } = true;
    public string Color { get; set; } = "#e0e0e0";
    public string Height { get; set; } = "2px";
    public string MarginTop { get; set; } = "8px";
    public string MarginBottom { get; set; } = "24px";
    public string Width { get; set; } = "100%";
}

public class LayoutSectionDto
{
    public string Type { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public string Title { get; set; } = string.Empty;
}
