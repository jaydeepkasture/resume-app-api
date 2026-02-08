using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Infrastructure.MigrationTools;

namespace ResumeInOneMinute.Controllers.Admin;

/// <summary>
/// Admin controller for database migrations and maintenance tasks
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[ApiController]
[Authorize] // You might want to add a specific admin role check here
public class MigrationController : ControllerBase
{
    private readonly IMongoDbService _mongoDbService;
    private readonly ILogger<ChatSessionMigrationTool> _logger;

    public MigrationController(
        IMongoDbService mongoDbService,
        ILogger<ChatSessionMigrationTool> logger)
    {
        _mongoDbService = mongoDbService;
        _logger = logger;
    }

    /// <summary>
    /// Perform a dry run of the chat session migration (no changes made)
    /// </summary>
    /// <returns>Migration preview results</returns>
    [HttpGet("migration/chat-sessions/dry-run")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> DryRunMigration()
    {
        var migrationTool = new ChatSessionMigrationTool(_mongoDbService, _logger);
        var result = await migrationTool.DryRunAsync();

        return Ok(new
        {
            Status = result.Success,
            Message = result.Success 
                ? $"Dry run completed. Would migrate {result.MigratedSessions} sessions with {result.MigratedMessages} messages"
                : $"Dry run failed: {result.ErrorMessage}",
            Data = new
            {
                result.TotalSessionsFound,
                result.MigratedSessions,
                result.MigratedMessages,
                FailedCount = result.FailedSessions.Count
            }
        });
    }

    /// <summary>
    /// Execute the chat session migration (WARNING: This will modify your database)
    /// </summary>
    /// <returns>Migration results</returns>
    [HttpPost("migration/chat-sessions/execute")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExecuteMigration()
    {
        var migrationTool = new ChatSessionMigrationTool(_mongoDbService, _logger);
        var result = await migrationTool.MigrateAsync();

        if (!result.Success)
        {
            return StatusCode(500, new
            {
                Status = false,
                Message = $"Migration failed: {result.ErrorMessage}",
                Data = (object?)null
            });
        }

        return Ok(new
        {
            Status = true,
            Message = $"Migration completed successfully! Migrated {result.MigratedSessions} sessions with {result.MigratedMessages} messages",
            Data = new
            {
                result.TotalSessionsFound,
                result.MigratedSessions,
                result.MigratedMessages,
                FailedSessions = result.FailedSessions,
                FailedCount = result.FailedSessions.Count
            }
        });
    }

    /// <summary>
    /// Get migration status and statistics
    /// </summary>
    [HttpGet("migration/chat-sessions/status")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMigrationStatus()
    {
        try
        {
            var database = _mongoDbService.GetDatabase();
            var chatSessionsRaw = database.GetCollection<MongoDB.Bson.BsonDocument>("chat_sessions");
            
            // Count sessions with embedded messages
            var filter = MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Exists("messages");
            var sessionsWithMessages = await chatSessionsRaw.CountDocumentsAsync(filter);
            
            // Count total sessions
            var totalSessions = await chatSessionsRaw.CountDocumentsAsync(MongoDB.Driver.Builders<MongoDB.Bson.BsonDocument>.Filter.Empty);
            
            // Count migrated sessions (those without messages field)
            var migratedSessions = totalSessions - sessionsWithMessages;

            return Ok(new
            {
                Status = true,
                Message = "Migration status retrieved",
                Data = new
                {
                    TotalChatSessions = totalSessions,
                    SessionsNeedingMigration = sessionsWithMessages,
                    SessionsAlreadyMigrated = migratedSessions,
                    MigrationComplete = sessionsWithMessages == 0
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting migration status");
            return StatusCode(500, new
            {
                Status = false,
                Message = $"Error getting migration status: {ex.Message}",
                Data = (object?)null
            });
        }
    }
}
