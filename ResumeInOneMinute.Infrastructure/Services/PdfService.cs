using PuppeteerSharp;
using PuppeteerSharp.Media;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;

namespace ResumeInOneMinute.Infrastructure.Services;

public class PdfService : IPdfService
{
    public async Task<PdfGenerationResponseDto> GenerateResumePdfAsync(PdfGenerationRequestDto request)
    {
        using var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();

        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true
        });

        using var page = await browser.NewPageAsync();
        await page.SetContentAsync(request.HtmlContent);

        var pdfData = await page.PdfDataAsync(new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions
            {
                Top = "10mm",
                Bottom = "10mm",
                Left = "10mm",
                Right = "10mm"
            }
        });

        await browser.CloseAsync();

        return new PdfGenerationResponseDto
        {
            PdfData = pdfData,
            FileName = request.FileName ?? "resume.pdf",
            ContentType = "application/pdf"
        };
    }
}
