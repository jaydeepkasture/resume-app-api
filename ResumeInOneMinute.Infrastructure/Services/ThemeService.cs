using MongoDB.Driver;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Infrastructure.Services;

public class ThemeService : IThemeService
{
    private readonly IMongoCollection<ResumeTheme> _themeCollection;

    public ThemeService(IMongoDbService mongoDbService)
    {
        _themeCollection = mongoDbService.GetCollection<ResumeTheme>("theme");
        
        // Ensure indexes
        var indexKeysDefinition = Builders<ResumeTheme>.IndexKeys.Ascending(t => t.LayoutType);
        _themeCollection.Indexes.CreateOne(new CreateIndexModel<ResumeTheme>(indexKeysDefinition));
        
        var activeIndexKeysDefinition = Builders<ResumeTheme>.IndexKeys.Ascending(t => t.IsActive);
        _themeCollection.Indexes.CreateOne(new CreateIndexModel<ResumeTheme>(activeIndexKeysDefinition));
    }

    public async Task<ThemePagedResultDto<ResumeTheme>> GetThemesAsync(int page, int pageSize, string layoutType)
    {
        var filter = Builders<ResumeTheme>.Filter.And(
            Builders<ResumeTheme>.Filter.Eq(t => t.LayoutType, layoutType),
            Builders<ResumeTheme>.Filter.Eq(t => t.IsActive, true)
        );

        var totalCount = await _themeCollection.CountDocumentsAsync(filter);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var items = await _themeCollection.Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .SortByDescending(t => t.CreatedAt)
            .ToListAsync();

        return new ThemePagedResultDto<ResumeTheme>
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            Items = items
        };
    }

    public async Task<ResumeTheme?> GetThemeByIdAsync(string id)
    {
        return await _themeCollection.Find(t => t.Id == id).FirstOrDefaultAsync();
    }

    public async Task<ResumeTheme> AddThemeAsync(ResumeThemeDto themeDto)
    {
        var theme = new ResumeTheme
        {
            Id = null,
            Name = themeDto.Name,
            LayoutType = themeDto.LayoutType,
            PreviewImage = themeDto.PreviewImage,
            Theme = themeDto.Theme,
            Decorations = themeDto.Decorations,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        await _themeCollection.InsertOneAsync(theme);
        return theme;
    }

    public async Task<IEnumerable<ResumeTheme>> AddThemesAsync(IEnumerable<ResumeThemeDto> themeDtos)
    {
        var themes = themeDtos.Select(dto => new ResumeTheme
        {
            Id = null,
            Name = dto.Name,
            LayoutType = dto.LayoutType,
            PreviewImage = dto.PreviewImage,
            Theme = dto.Theme,
            Decorations = dto.Decorations,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        }).ToList();

        await _themeCollection.InsertManyAsync(themes);
        return themes;
    }

    public async Task<bool> UpdateThemeAsync(string id, ResumeThemeDto themeDto)
    {
        var existingTheme = await GetThemeByIdAsync(id);
        if (existingTheme == null) return false;

        existingTheme.Name = themeDto.Name;
        existingTheme.LayoutType = themeDto.LayoutType;
        existingTheme.PreviewImage = themeDto.PreviewImage;
        existingTheme.Theme = themeDto.Theme;
        existingTheme.Decorations = themeDto.Decorations;
        // CreatedAt and IsActive are preserved

        var result = await _themeCollection.ReplaceOneAsync(t => t.Id == id, existingTheme);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteThemeAsync(string id)
    {
        var result = await _themeCollection.DeleteOneAsync(t => t.Id == id);
        return result.IsAcknowledged && result.DeletedCount > 0;
    }
}
