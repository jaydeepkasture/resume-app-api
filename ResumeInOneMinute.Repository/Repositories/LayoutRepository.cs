using MongoDB.Driver;
using ResumeInOneMinute.Domain.Constance;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Repository.Repositories;

public class LayoutRepository : ILayoutRepository
{
    private readonly IMongoCollection<Layout> _layoutsCollection;

    public LayoutRepository(IMongoDbService mongoDbService)
    {
        _layoutsCollection = mongoDbService.GetCollection<Layout>(MongoCollections.Layouts);
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        try
        {
            var layoutIdIndex = Builders<Layout>.IndexKeys.Ascending(x => x.LayoutId);
            var indexOptions = new CreateIndexOptions { Unique = true };
            _layoutsCollection.Indexes.CreateOne(new CreateIndexModel<Layout>(layoutIdIndex, indexOptions));
        }
        catch
        {
            // Index might already exist
        }
    }

    public async Task<ResponseList<LayoutDto>> GetLayoutsAsync(int page = 1, int pageSize = 10)
    {
        var filter = Builders<Layout>.Filter.Empty;
        var totalRecords = await _layoutsCollection.CountDocumentsAsync(filter);
        var layouts = await _layoutsCollection.Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return new ResponseList<LayoutDto>
        {
            Status = true,
            Message = "Layouts retrieved successfully",
            Data = layouts.Select(MapToDto).ToList(),
            TotalRecords = (int)totalRecords,
            RecordsFiltered = (int)totalRecords
        };
    }

    public async Task<Response<LayoutDto>> GetLayoutByIdAsync(string layoutId)
    {
        var filter = Builders<Layout>.Filter.Eq(x => x.LayoutId, layoutId);
        var layout = await _layoutsCollection.Find(filter).FirstOrDefaultAsync();

        if (layout == null)
        {
            return new Response<LayoutDto>
            {
                Status = false,
                Message = $"Layout with layoutId '{layoutId}' not found."
            };
        }

        return new Response<LayoutDto>
        {
            Status = true,
            Message = "Layout retrieved successfully",
            Data = MapToDto(layout)
        };
    }

    public async Task<Response<LayoutDto>> CreateLayoutAsync(LayoutDto dto)
    {
        var existing = await _layoutsCollection.Find(x => x.LayoutId == dto.LayoutId).FirstOrDefaultAsync();
        if (existing != null)
        {
            return new Response<LayoutDto>
            {
                Status = false,
                Message = $"Layout with layoutId '{dto.LayoutId}' already exists."
            };
        }

        var layout = MapToModel(dto);
        layout.CreatedAt = DateTime.UtcNow;
        layout.UpdatedAt = DateTime.UtcNow;

        await _layoutsCollection.InsertOneAsync(layout);

        return new Response<LayoutDto>
        {
            Status = true,
            Message = "Layout created successfully",
            Data = MapToDto(layout)
        };
    }

    public async Task SeedDefaultLayoutAsync()
    {
        var defaultLayoutId = "single-column-standard";
        var existing = await _layoutsCollection.Find(x => x.LayoutId == defaultLayoutId).FirstOrDefaultAsync();
        
        if (existing == null)
        {
            var defaultLayout = new LayoutDto
            {
                LayoutId = defaultLayoutId,
                Name = "Standard Single Column",
                Styles = new Dictionary<string, string>
                {
                    { "fontFamily", "'Inter', 'Roboto', sans-serif" },
                    { "primaryColor", "#1a1a1a" },
                    { "textPrimary", "#333333" },
                    { "headerBottom", "16px" },
                    { "sectionGap", "24px" }
                },
                Header = new LayoutHeaderDto
                {
                    Alignment = "center",
                    Name = new HeaderNameDto { Visible = true, Size = "36px", Weight = "700" },
                    Contact = new HeaderContactDto 
                    { 
                        Visible = true, 
                        Size = "14px", 
                        Separator = "|", 
                        VisibleFields = new List<string> { "location", "phoneNo", "email", "linkedIn", "gitHub" } 
                    },
                    Divider = new HeaderDividerDto
                    {
                        Enabled = true,
                        Color = "#e0e0e0",
                        Height = "2px",
                        MarginTop = "8px",
                        MarginBottom = "24px",
                        Width = "100%"
                    }
                },
                Sections = new List<LayoutSectionDto>
                {
                    new LayoutSectionDto { Type = "summary", Enabled = true, Title = "Professional Summary" },
                    new LayoutSectionDto { Type = "experience", Enabled = true, Title = "Work Experience" },
                    new LayoutSectionDto { Type = "education", Enabled = true, Title = "Education" },
                    new LayoutSectionDto { Type = "skills", Enabled = true, Title = "Core Skills" }
                }
            };
            
            await CreateLayoutAsync(defaultLayout);
        }
    }

    private LayoutDto MapToDto(Layout model)
    {
        return new LayoutDto
        {
            LayoutId = model.LayoutId,
            Name = model.Name,
            Styles = model.Styles,
            Header = new LayoutHeaderDto
            {
                Alignment = model.Header.Alignment,
                Name = new HeaderNameDto 
                { 
                    Visible = model.Header.Name.Visible, 
                    Size = model.Header.Name.Size, 
                    Weight = model.Header.Name.Weight 
                },
                Contact = new HeaderContactDto 
                { 
                    Visible = model.Header.Contact.Visible, 
                    Size = model.Header.Contact.Size, 
                    Separator = model.Header.Contact.Separator, 
                    VisibleFields = model.Header.Contact.VisibleFields 
                },
                Divider = new HeaderDividerDto 
                { 
                    Enabled = model.Header.Divider.Enabled, 
                    Color = model.Header.Divider.Color, 
                    Height = model.Header.Divider.Height, 
                    MarginTop = model.Header.Divider.MarginTop, 
                    MarginBottom = model.Header.Divider.MarginBottom, 
                    Width = model.Header.Divider.Width 
                }
            },
            Sections = model.Sections.Select(s => new LayoutSectionDto
            {
                Type = s.Type,
                Enabled = s.Enabled,
                Title = s.Title
            }).ToList()
        };
    }

    private Layout MapToModel(LayoutDto dto)
    {
        return new Layout
        {
            LayoutId = dto.LayoutId,
            Name = dto.Name,
            Styles = dto.Styles,
            Header = new LayoutHeader
            {
                Alignment = dto.Header.Alignment,
                Name = new LayoutHeaderName 
                { 
                    Visible = dto.Header.Name.Visible, 
                    Size = dto.Header.Name.Size, 
                    Weight = dto.Header.Name.Weight 
                },
                Contact = new LayoutHeaderContact 
                { 
                    Visible = dto.Header.Contact.Visible, 
                    Size = dto.Header.Contact.Size, 
                    Separator = dto.Header.Contact.Separator, 
                    VisibleFields = dto.Header.Contact.VisibleFields 
                },
                Divider = new LayoutHeaderDivider 
                { 
                    Enabled = dto.Header.Divider.Enabled, 
                    Color = dto.Header.Divider.Color, 
                    Height = dto.Header.Divider.Height, 
                    MarginTop = dto.Header.Divider.MarginTop, 
                    MarginBottom = dto.Header.Divider.MarginBottom, 
                    Width = dto.Header.Divider.Width 
                }
            },
            Sections = dto.Sections.Select(s => new LayoutSection
            {
                Type = s.Type,
                Enabled = s.Enabled,
                Title = s.Title
            }).ToList()
        };
    }
}
