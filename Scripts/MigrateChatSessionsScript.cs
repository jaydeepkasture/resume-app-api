using MongoDB.Bson;
using MongoDB.Driver;
using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Scripts;

/// <summary>
/// Migration script to convert old chat sessions with embedded messages
/// to the new structure with separate message storage.
/// 
/// Run this ONLY if you have existing chat sessions that need migration.
/// New chat sessions created after the refactoring don't need this.
/// </summary>
public class MigrateChatSessionsScript
{
    private readonly IMongoCollection<ChatSession> _chatSessionsCollection;
    private readonly IMongoCollection<ResumeEnhancementHistory> _historyCollection;

    public MigrateChatSessionsScript(
        IMongoCollection<ChatSession> chatSessionsCollection,
        IMongoCollection<ResumeEnhancementHistory> historyCollection)
    {
        _chatSessionsCollection = chatSessionsCollection;
        _historyCollection = historyCollection;
    }

    public async Task MigrateAsync()
    {
        Console.WriteLine("Starting migration of chat sessions...");
        
        // This will only work if you still have the old ChatSession model with Messages
        // Since we've already removed it, this is just for reference
        
        // If you need to run this, you would:
        // 1. Temporarily add back the Messages and CurrentResume fields to ChatSession
        // 2. Run this migration
        // 3. Remove the fields again
        
        Console.WriteLine("Migration completed!");
    }

    /// <summary>
    /// Alternative: Query old documents directly using BsonDocument
    /// This works without needing the old model
    /// </summary>
    public async Task MigrateUsingBsonAsync()
    {
        Console.WriteLine("Starting migration using BsonDocument...");
        
        var chatSessionsRaw = _chatSessionsCollection.Database
            .GetCollection<BsonDocument>("chat_sessions");
        
        // Find all chat sessions that have messages embedded
        var filter = Builders<BsonDocument>.Filter.Exists("messages");
        var oldSessions = await chatSessionsRaw.Find(filter).ToListAsync();
        
        Console.WriteLine($"Found {oldSessions.Count} chat sessions to migrate");
        
        int migratedCount = 0;
        
        foreach (var sessionDoc in oldSessions)
        {
            try
            {
                var chatId = sessionDoc["_id"].AsObjectId.ToString();
                var userId = sessionDoc["user_id"].AsInt64;
                
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
                                ResumeData = msg.Contains("resume_data") && !msg["resume_data"].IsBsonNull 
                                    ? msg["resume_data"].AsBsonDocument 
                                    : null,
                                CreatedAt = msg.Contains("timestamp") 
                                    ? msg["timestamp"].ToUniversalTime() 
                                    : DateTime.UtcNow,
                                ProcessingTimeMs = msg.Contains("processing_time_ms") && !msg["processing_time_ms"].IsBsonNull
                                    ? msg["processing_time_ms"].AsInt64
                                    : null
                            };
                            
                            await _historyCollection.InsertOneAsync(historyEntry);
                        }
                    }
                }
                
                // Remove messages and current_resume from chat session
                var update = Builders<BsonDocument>.Update
                    .Unset("messages")
                    .Unset("current_resume");
                
                await chatSessionsRaw.UpdateOneAsync(
                    Builders<BsonDocument>.Filter.Eq("_id", sessionDoc["_id"]),
                    update
                );
                
                migratedCount++;
                Console.WriteLine($"Migrated chat session {chatId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error migrating session: {ex.Message}");
            }
        }
        
        Console.WriteLine($"Migration completed! Migrated {migratedCount} chat sessions");
    }
}
