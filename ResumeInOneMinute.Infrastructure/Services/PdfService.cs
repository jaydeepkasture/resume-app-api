using System.Text;
using System.Text.Json;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using Microsoft.Extensions.Logging; // Added this using directive
using Microsoft.Extensions.Configuration; // Added this using directive

namespace ResumeInOneMinute.Infrastructure.Services;

public class PdfService : IPdfService
{
    private readonly IHtmlTemplateRepository _templateRepository;
    private readonly ILogger<PdfService> _logger; // Added this field
    private readonly IConfiguration _configuration;

    public PdfService(IHtmlTemplateRepository templateRepository, ILogger<PdfService> logger, IConfiguration configuration) // Modified constructor signature
    {
        _templateRepository = templateRepository;
        _logger = logger; // Added this assignment
        _configuration = configuration;
    }

    public async Task<PdfGenerationResponseDto> GenerateResumePdfAsync(PdfGenerationRequestDto request)
    {
        var template = await _templateRepository.GetTemplateByIdAsync(request.TemplateId);
        if (template == null)
        {
            throw new Exception("Template not found");
        }

        string rawHtml = template.HtmlTemplate;

        // Serialize resume data to JSON using camel case to match typical frontend expectations
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        string jsonResumeData = JsonSerializer.Serialize(request.Resume, options);

        // Clean up escaped backticks and dollar signs if they mistakenly got injected into the DB
        string cleanHtml = rawHtml.Replace("\\`", "`").Replace("\\$", "$");

        // Replace the placeholder in HTML with the actual JSON string
        string html = cleanHtml.Replace("{{RESUME_JSON_DATA}}", jsonResumeData);

        try
        {
            var executablePath = _configuration["PuppeteerSettings:ExecutablePath"];
            var downloadConfig = _configuration["PuppeteerSettings:DownloadBrowser"];
            var shouldDownload = !bool.TryParse(downloadConfig, out var download) || download;

            _logger.LogInformation("Puppeteer Setup - ExecutablePath: '{Path}', DownloadBrowser: {ShouldDownload} (Config Value: '{ConfigValue}')", 
                executablePath ?? "null", shouldDownload, downloadConfig ?? "null");

            if (string.IsNullOrEmpty(executablePath) && shouldDownload)
            {
                _logger.LogInformation("No executable path provided and DownloadBrowser is true. Attempting to download browser...");
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();
            }
            
            var launchOptions = new LaunchOptions
            {
                Headless = true,
                Args = new[] 
                { 
                    "--no-sandbox", 
                    "--disable-setuid-sandbox",
                    "--disable-gpu",
                    "--disable-dev-shm-usage",
                    "--disable-software-rasterizer",
                    "--disable-extensions",
                    "--remote-debugging-port=9222",
                    "--user-data-dir=/tmp/puppeteer_user_data", // Fix for Snap/Permission issues
                    "--disable-crash-reporter" // Fix for chrome_crashpad_handler error
                }
            };
            
            // Fix for mkdir: cannot create directory '/var/www/.local': Permission denied
            launchOptions.Env["HOME"] = "/tmp";

            if (!string.IsNullOrEmpty(executablePath))
            {
                _logger.LogInformation("Launching Puppeteer with configured path: {Path}", executablePath);
                launchOptions.ExecutablePath = executablePath;
            }
            else
            {
                _logger.LogInformation("Launching Puppeteer with default browser (expecting it to be downloaded or in PATH)");
            }

            await using var browser = await Puppeteer.LaunchAsync(launchOptions);

            await using var page = await browser.NewPageAsync();
            
            // Set content and wait until there is no network activity (so fonts/scripts load)
            await page.SetContentAsync(html, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 },
                Timeout = 60000 // 60 seconds timeout
            });
            
            // Make sure custom fonts are loaded
            await page.EvaluateFunctionAsync("async () => { await document.fonts.ready; }");

            // Fix: Inject CSS to override 'page-break-inside: avoid'
            await page.AddStyleTagAsync(new AddTagOptions
            {
                Content = @"
                    .item-block, .section, .skills-grid { 
                        page-break-inside: auto !important; 
                        break-inside: auto !important; 
                    }
                    .section-title, .item-header {
                        page-break-after: avoid !important;
                        break-after: avoid !important;
                    }
                    .item-header {
                        display: flex !important;
                        flex-direction: row !important;
                        justify-content: flex-start !important;
                        align-items: center !important;
                        gap: 2px !important;
                        flex-wrap: wrap !important;
                    }
                    .date-range { 
                        margin-left: 0 !important; 
                        display: flex !important;
                        align-items: center !important;
                    }
                    .separator {
                        display: inline-block !important;
                        margin: 0 8px !important;
                        visibility: visible !important;
                    }
                    @media print {
                        * {
                            -webkit-print-color-adjust: exact !important;
                            print-color-adjust: exact !important;
                        }
                    }
                "
            });

            var pdfData = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "40px",
                    Right = "40px",
                    Bottom = "40px",
                    Left = "40px"
                }
            });

            await browser.CloseAsync();

            return new PdfGenerationResponseDto
            {
                FileData = pdfData,
                FileName = request.FileName ?? $"{(string.IsNullOrWhiteSpace(request?.Resume?.Name) ? "resume" : request.Resume.Name.Replace(" ", "_"))}.pdf",
                ContentType = "application/pdf"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Puppeteer PDF generation failed");
            throw new Exception($"Puppeteer PDF generation failed: {ex.Message}", ex);
        }
    }
}
