using MongoDB.Bson;
using MongoDB.Driver;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;
using Microsoft.Extensions.Logging;

namespace ResumeInOneMinute.Infrastructure.MigrationTools;

/// <summary>
/// Migration tool to convert old chat sessions with embedded messages
/// to the new structure with separate message storage.
/// </summary>
public class ChatSessionMigrationTool
{
    private readonly IMongoCollection<BsonDocument> _chatSessionsRaw;
    private readonly IMongoCollection<ResumeEnhancementHistory> _historyCollection;
    private readonly ILogger<ChatSessionMigrationTool> _logger;

    public ChatSessionMigrationTool(
        IMongoDbService mongoDbService,
        ILogger<ChatSessionMigrationTool> logger)
    {
        _chatSessionsRaw = mongoDbService.GetDatabase().GetCollection<BsonDocument>("chat_sessions");
        _historyCollection = mongoDbService.GetCollection<ResumeEnhancementHistory>("resume_enhancement_history");
        _logger = logger;
    }

    /// <summary>
    /// Performs the migration of chat sessions with embedded messages
    /// </summary>
    public async Task<MigrationResult> MigrateAsync()
    {
        var result = new MigrationResult();
        
        _logger.LogInformation("Starting migration of chat sessions...");
        
        try
        {
            // Find all chat sessions that have messages embedded
            var filter = Builders<BsonDocument>.Filter.Exists("messages");
            var oldSessions = await _chatSessionsRaw.Find(filter).ToListAsync();
            
            _logger.LogInformation($"Found {oldSessions.Count} chat sessions with embedded messages to migrate");
            result.TotalSessionsFound = oldSessions.Count;
            
            foreach (var sessionDoc in oldSessions)
            {
                try
                {
                    await MigrateSingleSessionAsync(sessionDoc, result);
                }
                catch (Exception ex)
                {
                    var chatId = sessionDoc.Contains("_id") ? (sessionDoc["_id"].ToString() ?? "unknown") : "unknown";
                    _logger.LogError(ex, $"Error migrating session {chatId}: {ex.Message}");
                    result.FailedSessions.Add(new FailedSession
                    {
                        ChatId = chatId,
                        Error = ex.Message
                    });
                }
            }
            
            _logger.LogInformation($"Migration completed! Migrated {result.MigratedSessions} sessions, {result.MigratedMessages} messages");
            _logger.LogInformation($"Failed: {result.FailedSessions.Count} sessions");
            
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Migration failed: {ex.Message}");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }
        
        return result;
    }

    private async Task MigrateSingleSessionAsync(BsonDocument sessionDoc, MigrationResult result)
    {
        var chatId = sessionDoc["_id"].AsObjectId.ToString();
        var userId = sessionDoc["user_id"].AsInt64;
        
        _logger.LogInformation($"Migrating chat session {chatId} for user {userId}");
        
        int messageCount = 0;
        
        // Get embedded messages if they exist
        if (sessionDoc.Contains("messages") && sessionDoc["messages"].IsBsonArray)
        {
            var messages = sessionDoc["messages"].AsBsonArray;
            
            foreach (var msgDoc in messages)
            {
                if (msgDoc.IsBsonDocument)
                {
                    var msg = msgDoc.AsBsonDocument;
                    
                    // Create history entry for each message
                    var historyEntry = new ResumeEnhancementHistory
                    {
                        UserId = userId,
                        ChatId = chatId,
                        Role = msg.Contains("role") ? msg["role"].AsString : "user",
                        Message = msg.Contains("content") ? msg["content"].AsString : "",
                        CreatedAt = msg.Contains("timestamp") 
                            ? msg["timestamp"].ToUniversalTime() 
                            : DateTime.UtcNow,
                        ProcessingTimeMs = msg.Contains("processing_time_ms") && !msg["processing_time_ms"].IsBsonNull
                            ? msg["processing_time_ms"].AsInt64
                            : null
                    };
                    
                    await _historyCollection.InsertOneAsync(historyEntry);
                    messageCount++;
                }
            }
        }
        
        // Remove messages and current_resume from chat session
        var update = Builders<BsonDocument>.Update
            .Unset("messages")
            .Unset("current_resume");
        
        await _chatSessionsRaw.UpdateOneAsync(
            Builders<BsonDocument>.Filter.Eq("_id", sessionDoc["_id"]),
            update
        );
        
        result.MigratedSessions++;
        result.MigratedMessages += messageCount;
        
        _logger.LogInformation($"Migrated chat session {chatId}: {messageCount} messages");
    }

    /// <summary>
    /// Dry run - shows what would be migrated without actually doing it
    /// </summary>
    public async Task<MigrationResult> DryRunAsync()
    {
        var result = new MigrationResult();
        
        _logger.LogInformation("Starting DRY RUN of migration (no changes will be made)...");
        
        try
        {
            var filter = Builders<BsonDocument>.Filter.Exists("messages");
            var oldSessions = await _chatSessionsRaw.Find(filter).ToListAsync();
            
            _logger.LogInformation($"Found {oldSessions.Count} chat sessions with embedded messages");
            result.TotalSessionsFound = oldSessions.Count;
            
            foreach (var sessionDoc in oldSessions)
            {
                var chatId = sessionDoc.Contains("_id") ? sessionDoc["_id"].ToString() : "unknown";
                var userId = sessionDoc.Contains("user_id") ? sessionDoc["user_id"].AsInt64 : 0;
                
                int messageCount = 0;
                if (sessionDoc.Contains("messages") && sessionDoc["messages"].IsBsonArray)
                {
                    messageCount = sessionDoc["messages"].AsBsonArray.Count;
                }
                
                _logger.LogInformation($"Would migrate: ChatId={chatId}, UserId={userId}, Messages={messageCount}");
                result.MigratedSessions++;
                result.MigratedMessages += messageCount;
            }
            
            _logger.LogInformation($"DRY RUN completed! Would migrate {result.MigratedSessions} sessions, {result.MigratedMessages} messages");
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"DRY RUN failed: {ex.Message}");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }
        
        return result;
    }
}

public class MigrationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalSessionsFound { get; set; }
    public int MigratedSessions { get; set; }
    public int MigratedMessages { get; set; }
    public List<FailedSession> FailedSessions { get; set; } = new();
}

public class FailedSession
{
    public string ChatId { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}
