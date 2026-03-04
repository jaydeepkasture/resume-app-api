namespace ResumeInOneMinute.Domain.DTO;

public class PdfGenerationRequestDto
{
    public string TemplateId { get; set; } = string.Empty;
    public ResumeDto Resume { get; set; } = new();
    public string? FileName { get; set; }
}

public class PdfGenerationResponseDto
{
    public byte[] FileData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
}
