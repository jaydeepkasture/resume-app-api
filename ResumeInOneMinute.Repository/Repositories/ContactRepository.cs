using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;
using ResumeInOneMinute.Domain.Constance;

namespace ResumeInOneMinute.Repository.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly IMongoCollection<ContactMessage> _contactCollection;
    private readonly ILogger<ContactRepository> _logger;

    public ContactRepository(
        IMongoDbService mongoDbService,
        ILogger<ContactRepository> logger)
    {
        var database = mongoDbService.GetDatabase();
        _contactCollection = database.GetCollection<ContactMessage>(MongoCollections.ContactMessages);
        _logger = logger;
    }

    public async Task<Response<bool>> SaveContactMessageAsync(ContactMessageDto contactMessageDto)
    {
        try
        {
            var contactMessage = new ContactMessage
            {
                Name = contactMessageDto.Name.Trim(),
                Email = contactMessageDto.Email.Trim().ToLower(),
                Message = contactMessageDto.Message.Trim(),
                SubmittedAt = DateTime.UtcNow,
                IsResolved = false
            };

            await _contactCollection.InsertOneAsync(contactMessage);

            _logger.LogInformation("Contact message saved from {Email}", contactMessage.Email);

            return new Response<bool>
            {
                Status = true,
                Message = "Your message has been sent successfully. We will get back to you soon.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving contact message from {Email}", contactMessageDto.Email);
            return new Response<bool>
            {
                Status = false,
                Message = "An error occurred while sending your message. Please try again later.",
                Data = false
            };
        }
    }
}
