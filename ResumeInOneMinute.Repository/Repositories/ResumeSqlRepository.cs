using MongoDB.Driver;
using ResumeInOneMinute.Domain.Constance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;
using ResumeInOneMinute.Repository.Repositories.Base;

namespace ResumeInOneMinute.Repository.Repositories;

public class ResumeSqlRepository : BaseRepository, IResumeSqlRepository
{
    private readonly IMongoCollection<Resume> _resumeCollection;

    public ResumeSqlRepository(IConfiguration configuration, IMongoDbService mongoDbService) : base(configuration)
    {
        _resumeCollection = mongoDbService.GetCollection<Resume>(MongoCollections.Resume);
    }

    public async Task<Response<ResumeDto>> GetResumeByIdAsync(long userId)
    {
        var filter = Builders<Resume>.Filter.Eq(r => r.UserId, userId);
        var resume = await _resumeCollection.Find(filter).FirstOrDefaultAsync();

        if (resume == null)
        {
            return new Response<ResumeDto>
            {
                Status = false,
                Message = "Resume not found"
            };
        }

        return new Response<ResumeDto>
        {
            Status = true,
            Message = "Resume retrieved successfully",
            Data = resume.ResumeData
        };
    }

    public async Task<Response<long>> SaveResumeAsync(long userId, long templateId, ResumeDto resumeData)
    {
        using var context = CreateDbContext();
        
        // Check if template exists
        var templateExists = await context.ResumeTemplates.AnyAsync(t => t.Id == templateId);
        if (!templateExists)
        {
            return new Response<long>
            {
                Status = false,
                Message = "Specified template does not exist"
            };
        }

        var resume = new UserResume
        {
            UserId = userId,
            TemplateId = templateId,
            ResumeData = resumeData,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.UserResumes.Add(resume);
        await context.SaveChangesAsync();

        return new Response<long>
        {
            Status = true,
            Message = "Resume saved successfully",
            Data = resume.Id
        };
    }
}
