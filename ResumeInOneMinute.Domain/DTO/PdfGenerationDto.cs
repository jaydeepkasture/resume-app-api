namespace ResumeInOneMinute.Domain.DTO;

public class PdfGenerationRequestDto
{
    public string ThemeId { get; set; } = string.Empty;
    public ResumeDto Resume { get; set; } = new();
}

public class PdfGenerationResponseDto
{
    public string FileName { get; set; } = "resume.pdf";
    public string ContentType { get; set; } = "application/pdf";
    public string FileData { get; set; } = string.Empty;
}
