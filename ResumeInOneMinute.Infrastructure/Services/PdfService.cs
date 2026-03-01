using System.Text;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Infrastructure.Services;

public class PdfService : IPdfService
{
    private readonly IThemeService _themeService;

    public PdfService(IThemeService themeService)
    {
        _themeService = themeService;
    }

    public async Task<PdfGenerationResponseDto> GenerateResumePdfAsync(PdfGenerationRequestDto request)
    {
        var theme = await _themeService.GetThemeByIdAsync(request.ThemeId);
        if (theme == null)
        {
            throw new Exception("Theme not found");
        }

        if (theme.LayoutType != "single-column")
        {
            throw new Exception($"Layout type '{theme.LayoutType}' is not supported yet.");
        }

        string html = BuildHtml(theme, request.Resume);
        byte[] pdfBytes = await GeneratePdfFromHtmlAsync(html);

        return new PdfGenerationResponseDto
        {
            FileName = $"{request.Resume.Name.Replace(" ", "_")}_Resume.pdf",
            FileData = Convert.ToBase64String(pdfBytes)
        };
    }

    private string BuildHtml(ResumeTheme theme, ResumeDto resume)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><meta charset='UTF-8'>");
        sb.Append("<style>");
        sb.Append(GenerateCss(theme));
        sb.Append("</style></head><body>");
        sb.Append("<div class='resume'>");

        // Header
        sb.Append("<div class='header'>");
        sb.Append($"<h1>{System.Net.WebUtility.HtmlEncode(resume.Name)}</h1>");
        
        var contactInfo = new List<string>();
        if (!string.IsNullOrEmpty(resume.Location)) contactInfo.Add(System.Net.WebUtility.HtmlEncode(resume.Location));
        if (!string.IsNullOrEmpty(resume.PhoneNo)) contactInfo.Add(System.Net.WebUtility.HtmlEncode(resume.PhoneNo));
        if (!string.IsNullOrEmpty(resume.Email)) contactInfo.Add(System.Net.WebUtility.HtmlEncode(resume.Email));
        if (!string.IsNullOrEmpty(resume.LinkedIn)) contactInfo.Add(System.Net.WebUtility.HtmlEncode(resume.LinkedIn));
        if (!string.IsNullOrEmpty(resume.GitHub)) contactInfo.Add(System.Net.WebUtility.HtmlEncode(resume.GitHub));
        
        sb.Append($"<p>{string.Join(" | ", contactInfo)}</p>");
        sb.Append("</div>");

        // Header Divider
        if (theme.Decorations?.HeaderDivider?.Enabled == true)
        {
            var d = theme.Decorations.HeaderDivider;
            sb.Append($"<div style='height:{d.Height}; background:{d.Color}; margin-top:{d.MarginTop}; margin-bottom:{d.MarginBottom};'></div>");
        }

        // Summary
        if (!string.IsNullOrEmpty(resume.Summary))
        {
            sb.Append("<section class='section'>");
            sb.Append("<h2>Profile</h2>");
            sb.Append($"<p>{System.Net.WebUtility.HtmlEncode(resume.Summary)}</p>");
            sb.Append("</section>");
        }

        // Experience
        if (resume.Experience != null && resume.Experience.Count > 0)
        {
            sb.Append("<section class='section'>");
            sb.Append("<h2>Experience</h2>");
            foreach (var exp in resume.Experience)
            {
                sb.Append("<div class='item'>");
                sb.Append($"<strong>{System.Net.WebUtility.HtmlEncode(exp.Position)} | {System.Net.WebUtility.HtmlEncode(exp.Company)} | {System.Net.WebUtility.HtmlEncode(exp.From)} - {System.Net.WebUtility.HtmlEncode(exp.To)}</strong>");
                
                if (exp.Description != null && exp.Description.Count > 0)
                {
                    sb.Append("<ul style='margin-top: 5px;'>");
                    foreach (var line in exp.Description)
                    {
                        sb.Append($"<li>{System.Net.WebUtility.HtmlEncode(line)}</li>");
                    }
                    sb.Append("</ul>");
                }
                sb.Append("</div>");
            }
            sb.Append("</section>");
        }

        // Education
        if (resume.Education != null && resume.Education.Count > 0)
        {
            sb.Append("<section class='section'>");
            sb.Append("<h2>Education</h2>");
            foreach (var edu in resume.Education)
            {
                sb.Append("<div class='item'>");
                sb.Append($"<strong>{System.Net.WebUtility.HtmlEncode(edu.Degree)} in {System.Net.WebUtility.HtmlEncode(edu.Field)} | {System.Net.WebUtility.HtmlEncode(edu.Year)} | {System.Net.WebUtility.HtmlEncode(edu.Institution)}</strong>");
                sb.Append("</div>");
            }
            sb.Append("</section>");
        }

        // Skills
        if (resume.Skills != null && resume.Skills.Count > 0)
        {
            sb.Append("<section class='section'>");
            sb.Append("<h2>Skills</h2>");
            sb.Append("<ul class='skills-list'>");
            foreach (var skill in resume.Skills)
            {
                sb.Append($"<li>{System.Net.WebUtility.HtmlEncode(skill)}</li>");
            }
            sb.Append("</ul>");
            sb.Append("</section>");
        }

        sb.Append("</div></body></html>");
        return sb.ToString();
    }

    private string GenerateCss(ResumeTheme theme)
    {
        var t = theme.Theme;
        return $@"
            body {{
                font-family: {t.Typography.FontFamily};
                background-color: {t.Colors.Background};
                color: {t.Colors.TextPrimary};
                margin: 0;
                padding: 0;
                line-height: {t.Typography.Body.LineHeight ?? "1.5"};
                font-size: {t.Typography.Body.Size};
            }}
            .resume {{
                max-width: 800px;
                margin: 0 auto;
            }}
            .header {{
                text-align: center;
                margin-bottom: {t.Spacing.HeaderBottom};
            }}
            .header h1 {{
                margin: 0;
                color: {t.Colors.Primary};
                font-size: {t.Typography.Name.Size};
                font-weight: {t.Typography.Name.Weight ?? "bold"};
            }}
            .header p {{
                margin: 5px 0 0;
                font-size: {t.Typography.Contact.Size};
            }}
            .section {{
                margin-top: {t.Spacing.SectionGap};
                page-break-inside: avoid;
            }}
            .section h2 {{
                margin: 0 0 {t.Spacing.ParagraphGap};
                color: {t.Colors.Primary};
                font-size: {t.Typography.SectionTitle.Size};
                font-weight: {t.Typography.SectionTitle.Weight ?? "bold"};
                border-bottom: 1px solid #eee;
                padding-bottom: 5px;
                text-transform: uppercase;
            }}
            .item {{
                margin-bottom: {t.Spacing.ParagraphGap};
                page-break-inside: avoid;
            }}
            .item strong {{
                display: block;
                margin-bottom: 2px;
            }}
            .item p {{
                margin: 0;
                white-space: pre-line;
            }}
            .skills-list {{
                column-count: 2;
                padding-left: 20px;
                margin: 0;
                page-break-inside: avoid;
            }}
            .skills-list li {{
                margin-bottom: 5px;
            }}
        ";
    }

    private async Task<byte[]> GeneratePdfFromHtmlAsync(string html)
    {
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        });

        await using var page = await browser.NewPageAsync();
        await page.SetContentAsync(html);
        
        // Wait for fonts/images to load if any
        await page.EvaluateExpressionAsync("document.fonts.ready");

        return await page.PdfDataAsync(new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            PreferCSSPageSize = true,
            MarginOptions = new MarginOptions
            {
                Top = "40px",
                Right = "40px",
                Bottom = "40px",
                Left = "40px"
            }
        });
    }
}
