using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;
using ResumeInOneMinute.Repository.Repositories.Base;

namespace ResumeInOneMinute.Repository.Repositories;

public class ResumeTemplateRepository : BaseRepository, IResumeTemplateRepository
{
    public ResumeTemplateRepository(IConfiguration configuration) : base(configuration)
    {
    }

    public async Task<Response<ResumeTemplateDto>> GetByIdAsync(long id)
    {
        using var context = CreateDbContext();
        var template = await context.ResumeTemplates
            .FirstOrDefaultAsync(t => t.Id == id);

        if (template == null)
        {
            return new Response<ResumeTemplateDto>
            {
                Status = false,
                Message = "Template not found"
            };
        }

        return new Response<ResumeTemplateDto>
        {
            Status = true,
            Message = "Template retrieved successfully",
            Data = MapToDto(template)
        };
    }

    public async Task<Response<List<ResumeTemplateListDto>>> GetActiveTemplatesAsync()
    {
        using var context = CreateDbContext();
        var templates = await context.ResumeTemplates
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();

        var dtos = templates.Select(t => new ResumeTemplateListDto
        {
            Id = t.Id,
            Name = t.Name,
            LayoutType = t.LayoutType,
            SectionOrder = t.SectionOrder,
            Theme = t.Theme,
            Decorations = t.Decorations
        }).ToList();

        return new Response<List<ResumeTemplateListDto>>
        {
            Status = true,
            Message = "Active templates retrieved successfully",
            Data = dtos
        };
    }

    public async Task<Response<ResumeTemplateDto>> CreateAsync(CreateResumeTemplateDto dto)
    {
        using var context = CreateDbContext();
        
        var template = new ResumeTemplate
        {
            Name = dto.Name,
            LayoutType = dto.LayoutType,
            SectionOrder = dto.SectionOrder,
            Theme = dto.Theme,
            Decorations = dto.Decorations,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.ResumeTemplates.Add(template);
        await context.SaveChangesAsync();

        return new Response<ResumeTemplateDto>
        {
            Status = true,
            Message = "Template created successfully",
            Data = MapToDto(template)
        };
    }

    private static ResumeTemplateDto MapToDto(ResumeTemplate template)
    {
        return new ResumeTemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            LayoutType = template.LayoutType,
            SectionOrder = template.SectionOrder,
            Theme = template.Theme,
            Decorations = template.Decorations,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }
}
