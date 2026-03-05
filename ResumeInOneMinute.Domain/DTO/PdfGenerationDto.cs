namespace ResumeInOneMinute.Domain.DTO;

public class PdfGenerationRequestDto
{
    public string HtmlContent { get; set; } = string.Empty;
    public string FileName { get; set; } = "resume.pdf";
}

public class PdfGenerationResponseDto
{
    public byte[] PdfData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
}
