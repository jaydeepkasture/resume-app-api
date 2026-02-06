using MongoDB.Driver;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;
using Microsoft.Extensions.Logging;

namespace ResumeInOneMinute.Repository.Repositories;

public class HtmlTemplateRepository : IHtmlTemplateRepository
{
    private readonly IMongoCollection<HtmlTemplate> _templatesCollection;
    private readonly ILogger<HtmlTemplateRepository> _logger;

    public HtmlTemplateRepository(
        IMongoDbService mongoDbService,
        ILogger<HtmlTemplateRepository> logger)
    {
        var database = mongoDbService.GetDatabase();
        _templatesCollection = database.GetCollection<HtmlTemplate>("html_templates");
        _logger = logger;
    }

    public async Task<HtmlTemplateResponseDto> CreateTemplateAsync(CreateHtmlTemplateDto dto, string userId)
    {
        try
        {
            var template = new HtmlTemplate
            {
                HtmlTemplateContent = dto.HtmlTemplate,
                TemplateName = dto.TemplateName,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                TemplateTypeId = dto.TemplateTypeId,
                IsActive = true
            };

            await _templatesCollection.InsertOneAsync(template);

            _logger.LogInformation("Created HTML template with ID: {TemplateId} by user: {UserId}", 
                template.Id, userId);

            return MapToResponseDto(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating HTML template for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<PaginatedHtmlTemplatesDto> GetTemplatesAsync(
        int page = 1, 
        int pageSize = 10, 
        string? search = null,
        int? templateTypeId = null)
    {
        try
        {
            var filterBuilder = Builders<HtmlTemplate>.Filter;
            var filter = filterBuilder.Eq(x=>x.IsActive,true);

            // Add template type filter if provided
            if (templateTypeId.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(t => t.TemplateTypeId, templateTypeId.Value));
            }

            // Add search filter if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchFilter = filterBuilder.Or(
                    filterBuilder.Regex(t => t.TemplateName, new MongoDB.Bson.BsonRegularExpression(search, "i")),
                    filterBuilder.Regex(t => t.HtmlTemplateContent, new MongoDB.Bson.BsonRegularExpression(search, "i"))
                );
                filter = filterBuilder.And(filter, searchFilter);
            }

            // Get total count
            var totalCount = await _templatesCollection.CountDocumentsAsync(filter);

            // Get paginated results
            var templates = await _templatesCollection
                .Find(filter)
                .SortByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            var templateDtos = templates.Select(t => new HtmlTemplateListDto
            {
                Id = t.Id,
                HtmlTemplate = t.HtmlTemplateContent,
                TemplateName = t.TemplateName,
                CreatedAt = t.CreatedAt,
                CreatedBy = t.CreatedBy,
                TemplateTypeId = t.TemplateTypeId
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            _logger.LogInformation("Retrieved {Count} templates (page {Page} of {TotalPages})", 
                templateDtos.Count, page, totalPages);

            return new PaginatedHtmlTemplatesDto
            {
                Templates = templateDtos,
                TotalCount = (int)totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HTML templates");
            throw;
        }
    }

    public async Task<HtmlTemplateResponseDto?> GetTemplateByIdAsync(string templateId)
    {
        try
        {
            var filter = Builders<HtmlTemplate>.Filter.Eq(t => t.Id, templateId);
            var template = await _templatesCollection.Find(filter).FirstOrDefaultAsync();

            if (template == null)
            {
                _logger.LogWarning("HTML template not found: {TemplateId}", templateId);
                return null;
            }

            _logger.LogInformation("Retrieved HTML template: {TemplateId}", templateId);
            return MapToResponseDto(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving HTML template: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<bool> DeleteTemplateAsync(string templateId)
    {
        try
        {
            var filter = Builders<HtmlTemplate>.Filter.Eq(t => t.Id, templateId);
            var result = await _templatesCollection.DeleteOneAsync(filter);

            if (result.DeletedCount > 0)
            {
                _logger.LogInformation("Deleted HTML template: {TemplateId}", templateId);
                return true;
            }

            _logger.LogWarning("HTML template not found for deletion: {TemplateId}", templateId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting HTML template: {TemplateId}", templateId);
            throw;
        }
    }

    private HtmlTemplateResponseDto MapToResponseDto(HtmlTemplate template)
    {
        return new HtmlTemplateResponseDto
        {
            Id = template.Id,
            HtmlTemplate = template.HtmlTemplateContent,
            TemplateName = template.TemplateName,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            CreatedBy = template.CreatedBy,
            TemplateTypeId = template.TemplateTypeId
        };
    }
}
