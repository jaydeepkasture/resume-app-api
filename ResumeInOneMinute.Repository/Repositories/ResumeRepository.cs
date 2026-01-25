using MongoDB.Bson;
using MongoDB.Driver;
using ResumeInOneMinute.Domain.Constance;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;
using System.Diagnostics;
using System.Text.Json;

namespace ResumeInOneMinute.Repository.Repositories;

public class ResumeRepository : IResumeRepository
{
    private readonly IMongoCollection<ResumeEnhancementHistory> _historyCollection;
    private readonly IMongoCollection<ChatSession> _chatSessionsCollection;
    private readonly IOllamaService _ollamaService;

    public ResumeRepository(IMongoDbService mongoDbService, IOllamaService ollamaService)
    {
        _historyCollection = mongoDbService.GetCollection<ResumeEnhancementHistory>(MongoCollections.ResumeEnhancementHistory);
        _chatSessionsCollection = mongoDbService.GetCollection<ChatSession>(MongoCollections.ChatSessions);
        _ollamaService = ollamaService;
        
        // Create indexes for better query performance
        CreateIndexes();
    }

    public async Task<Response<ResumeEnhancementResponseDto>> EnhanceResumeAsync(long userId, ResumeEnhancementRequestDto request)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Call Ollama to enhance the resume
            var enhancedResume = await _ollamaService.EnhanceResumeAsync(
                request.ResumeData, 
                request.EnhancementInstruction
            );
            
            stopwatch.Stop();
            
            // Save to MongoDB history
            var history = new ResumeEnhancementHistory
            {
                UserId = userId,
                OriginalResume = ConvertToBsonDocument(request.ResumeData),
                EnhancedResume = ConvertToBsonDocument(enhancedResume),
                EnhancementInstruction = request.EnhancementInstruction,
                CreatedAt = DateTime.UtcNow,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
            
            await _historyCollection.InsertOneAsync(history);
            
            var response = new ResumeEnhancementResponseDto
            {
                OriginalResume = request.ResumeData,
                EnhancedResume = enhancedResume,
                EnhancementInstruction = request.EnhancementInstruction,
                HistoryId = history.Id,
                ProcessedAt = history.CreatedAt
            };
            
            return new Response<ResumeEnhancementResponseDto>
            {
                Status = true,
                Message = $"Resume enhanced successfully in {stopwatch.ElapsedMilliseconds}ms",
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new Response<ResumeEnhancementResponseDto>
            {
                Status = false,
                Message = $"Failed to enhance resume: {ex.Message}",
                Data = null!
            };
        }
    }

    public async Task<Response<List<ResumeEnhancementResponseDto>>> GetUserHistoryAsync(long userId, int page = 1, int pageSize = 10)
    {
        try
        {
            var skip = (page - 1) * pageSize;
            
            var filter = Builders<ResumeEnhancementHistory>.Filter.Eq(h => h.UserId, userId);
            var sort = Builders<ResumeEnhancementHistory>.Sort.Descending(h => h.CreatedAt);
            
            var histories = await _historyCollection
                .Find(filter)
                .Sort(sort)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
            
            var responseList = histories.Select(h => new ResumeEnhancementResponseDto
            {
                OriginalResume = ConvertToResumeDto(h.OriginalResume),
                EnhancedResume = ConvertToResumeDto(h.EnhancedResume),
                EnhancementInstruction = h.EnhancementInstruction,
                HistoryId = h.Id,
                ProcessedAt = h.CreatedAt,
                TemplateId = h.TemplateId
            }).ToList();
            
            return new Response<List<ResumeEnhancementResponseDto>>
            {
                Status = true,
                Message = $"Retrieved {responseList.Count} history records",
                Data = responseList
            };
        }
        catch (Exception ex)
        {
            return new Response<List<ResumeEnhancementResponseDto>>
            {
                Status = false,
                Message = $"Failed to retrieve history: {ex.Message}",
                Data = new List<ResumeEnhancementResponseDto>()
            };
        }
    }

    public async Task<Response<ResumeEnhancementResponseDto>> GetHistoryByIdAsync(long userId, string historyId)
    {
        try
        {
            var filter = Builders<ResumeEnhancementHistory>.Filter.And(
                Builders<ResumeEnhancementHistory>.Filter.Eq(h => h.Id, historyId),
                Builders<ResumeEnhancementHistory>.Filter.Eq(h => h.UserId, userId)
            );
            
            var history = await _historyCollection.Find(filter).FirstOrDefaultAsync();
            
            if (history == null)
            {
                return new Response<ResumeEnhancementResponseDto>
                {
                    Status = false,
                    Message = "History record not found",
                    Data = null!
                };
            }
            
            var response = new ResumeEnhancementResponseDto
            {
                OriginalResume = ConvertToResumeDto(history.OriginalResume),
                EnhancedResume = ConvertToResumeDto(history.EnhancedResume),
                EnhancementInstruction = history.EnhancementInstruction,
                HistoryId = history.Id,
                ProcessedAt = history.CreatedAt,
                TemplateId = history.TemplateId
            };
            
            return new Response<ResumeEnhancementResponseDto>
            {
                Status = true,
                Message = "History record retrieved successfully",
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new Response<ResumeEnhancementResponseDto>
            {
                Status = false,
                Message = $"Failed to retrieve history: {ex.Message}",
                Data = null!
            };
        }
    }

    #region Private Helper Methods

    private void CreateIndexes()
    {
        try
        {
            // History collection indexes
            // Create index on UserId for faster queries
            var userIdIndex = Builders<ResumeEnhancementHistory>.IndexKeys.Ascending(h => h.UserId);
            _historyCollection.Indexes.CreateOne(new CreateIndexModel<ResumeEnhancementHistory>(userIdIndex));
            
            // Create compound index on UserId and CreatedAt
            var userCreatedIndex = Builders<ResumeEnhancementHistory>.IndexKeys
                .Ascending(h => h.UserId)
                .Descending(h => h.CreatedAt);
            _historyCollection.Indexes.CreateOne(new CreateIndexModel<ResumeEnhancementHistory>(userCreatedIndex));
            
            // Create compound index on ChatId and UserId for chat message queries
            var chatUserIndex = Builders<ResumeEnhancementHistory>.IndexKeys
                .Ascending(h => h.ChatId)
                .Ascending(h => h.UserId);
            _historyCollection.Indexes.CreateOne(new CreateIndexModel<ResumeEnhancementHistory>(chatUserIndex));
            
            // Create compound index on ChatId and CreatedAt for ordered message retrieval
            var chatCreatedIndex = Builders<ResumeEnhancementHistory>.IndexKeys
                .Ascending(h => h.ChatId)
                .Ascending(h => h.CreatedAt);
            _historyCollection.Indexes.CreateOne(new CreateIndexModel<ResumeEnhancementHistory>(chatCreatedIndex));
            
            // Chat sessions collection indexes
            var chatUserIdIndex = Builders<ChatSession>.IndexKeys.Ascending(c => c.UserId);
            _chatSessionsCollection.Indexes.CreateOne(new CreateIndexModel<ChatSession>(chatUserIdIndex));
            
            var chatUpdatedIndex = Builders<ChatSession>.IndexKeys
                .Ascending(c => c.UserId)
                .Descending(c => c.UpdatedAt);
            _chatSessionsCollection.Indexes.CreateOne(new CreateIndexModel<ChatSession>(chatUpdatedIndex));
        }
        catch
        {
            // Indexes might already exist, ignore errors
        }
    }

    private BsonDocument ConvertToBsonDocument(ResumeDto resume)
    {
        var json = JsonSerializer.Serialize(resume, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return BsonDocument.Parse(json);
    }

    private ResumeDto ConvertToResumeDto(BsonDocument bsonDocument)
    {
        var json = bsonDocument.ToJson();
        return JsonSerializer.Deserialize<ResumeDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new ResumeDto();
    }

    #region Chat-Based Enhancement Methods

    public async Task<Response<ChatSessionSummaryDto>> CreateChatSessionAsync(long userId, CreateChatSessionDto request)
    {
        try
        {
            // Create lightweight chat session (metadata only)
            var chatSession = new ChatSession
            {
                UserId = userId,
                Title = request.Title ?? "New Chat",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _chatSessionsCollection.InsertOneAsync(chatSession);

            // If initial resume is provided, create the first history entry
            if (request.InitialResume != null)
            {
                var initialHistory = new ResumeEnhancementHistory
                {
                    UserId = userId,
                    ChatId = chatSession.Id,
                    Role = "system",
                    Message = "Initial resume created",
                    OriginalResume = ConvertToBsonDocument(request.InitialResume),
                    CreatedAt = DateTime.UtcNow
                };
                
                await _historyCollection.InsertOneAsync(initialHistory);
            }

            return new Response<ChatSessionSummaryDto>
            {
                Status = true,
                Message = "Chat session created successfully",
                Data = new ChatSessionSummaryDto
                {
                    ChatId = chatSession.Id,
                    Title = chatSession.Title,
                    CreatedAt = chatSession.CreatedAt,
                    UpdatedAt = chatSession.UpdatedAt,
                    MessageCount = request.InitialResume != null ? 1 : 0,
                    IsActive = chatSession.IsActive
                }
            };
        }
        catch (Exception ex)
        {
            return new Response<ChatSessionSummaryDto>
            {
                Status = false,
                Message = $"Failed to create chat session: {ex.Message}",
                Data = null!
            };
        }
    }

    public async Task<Response<ChatEnhancementResponseDto>> ChatEnhanceAsync(long userId, ChatEnhancementRequestDto request)
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Get or create chat session (metadata only)
            ChatSession? chatSession;
            if (!string.IsNullOrEmpty(request.ChatId))
            {
                var filter = Builders<ChatSession>.Filter.And(
                    Builders<ChatSession>.Filter.Eq(c => c.Id, request.ChatId),
                    Builders<ChatSession>.Filter.Eq(c => c.UserId, userId)
                );
                chatSession = await _chatSessionsCollection.Find(filter).FirstOrDefaultAsync();

                if (chatSession == null)
                {
                    return new Response<ChatEnhancementResponseDto>
                    {
                        Status = false,
                        Message = "Chat session not found",
                        Data = null!
                    };
                }
            }
            else
            {
                // Create new chat session (metadata only)
                chatSession = new ChatSession
                {
                    UserId = userId,
                    Title = "New Chat",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                await _chatSessionsCollection.InsertOneAsync(chatSession);
            }

            // Get chat history from resume_enhancement_history collection
            var historyFilter = Builders<ResumeEnhancementHistory>.Filter.And(
                Builders<ResumeEnhancementHistory>.Filter.Eq(h => h.ChatId, chatSession.Id),
                Builders<ResumeEnhancementHistory>.Filter.Eq(h => h.UserId, userId)
            );
            var chatHistory = await _historyCollection
                .Find(historyFilter)
                .SortBy(h => h.CreatedAt)
                .ToListAsync();

            // Determine current resume (from request or latest in history)
            ResumeDto? originalResume = request.ResumeData;
            string? originalHtml = request.ResumeHtml;
            
            if (originalResume == null)
            {
                // Get the most recent enhanced resume from history
                var latestResumeEntry = chatHistory
                    .Where(h => h.EnhancedResume != null)
                    .OrderByDescending(h => h.CreatedAt)
                    .FirstOrDefault();
                
                if (latestResumeEntry?.EnhancedResume != null)
                {
                    originalResume = ConvertToResumeDto(latestResumeEntry.EnhancedResume);
                }
                
                // Also get the latest HTML if not provided
                if (string.IsNullOrEmpty(originalHtml))
                {
                    var latestHtmlEntry = chatHistory
                        .Where(h => !string.IsNullOrEmpty(h.EnhancedHtml))
                        .OrderByDescending(h => h.CreatedAt)
                        .FirstOrDefault();
                    
                    originalHtml = latestHtmlEntry?.EnhancedHtml;
                }
            }

            // Build context from chat history for AI
            var conversationContext = BuildConversationContextFromHistory(chatHistory, request.Message, originalResume);

            // Call Ollama with conversation context
            string aiResponse;
            ResumeDto? enhancedResume = null;
            string? enhancedHtml = null;

            if (originalResume != null)
            {
                // Check if we should use HTML enhancement
                if (!string.IsNullOrEmpty(originalHtml))
                {
                    // HTML-based enhancement (for TiptapAngular editor)
                    var (html, resume) = await _ollamaService.EnhanceResumeHtmlAsync(
                        originalHtml, 
                        originalResume, 
                        request.Message);
                    
                    enhancedHtml = html;
                    enhancedResume = resume;
                    aiResponse = "I've enhanced your resume based on your request. The updated HTML is ready to be displayed in the editor.";
                }
                else
                {
                    // JSON-only enhancement (legacy)
                    enhancedResume = await _ollamaService.EnhanceResumeAsync(originalResume, conversationContext);
                    aiResponse = "I've enhanced your resume based on your request. Here's the updated version.";
                }
            }
            else
            {
                // Just conversational response (no resume to enhance yet)
                aiResponse = await GetConversationalResponseAsync(conversationContext);
            }

            stopwatch.Stop();

            // Save ONE complete enhancement entry to history
            var enhancementEntry = new ResumeEnhancementHistory
            {
                UserId = userId,
                ChatId = chatSession.Id,
                TemplateId = request.TemplateId,
                Message = request.Message,
                AssistantMessage = aiResponse,
                OriginalResume = originalResume != null ? ConvertToBsonDocument(originalResume) : null,
                EnhancedResume = enhancedResume != null ? ConvertToBsonDocument(enhancedResume) : null,
                ResumeHtml = originalHtml,
                EnhancedHtml = enhancedHtml,
                CreatedAt = DateTime.UtcNow,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
            await _historyCollection.InsertOneAsync(enhancementEntry);

            // Update chat session metadata only
            chatSession.UpdatedAt = DateTime.UtcNow;
            
            // Auto-generate title from first message if still "New Chat"
            var messageCount = chatHistory.Count + 1; // +1 for the entry we just added
            if (chatSession.Title == "New Chat" && messageCount == 1)
            {
                chatSession.Title = GenerateChatTitle(request.Message);
            }

            var update = Builders<ChatSession>.Update
                .Set(c => c.UpdatedAt, chatSession.UpdatedAt)
                .Set(c => c.Title, chatSession.Title);

            await _chatSessionsCollection.UpdateOneAsync(
                c => c.Id == chatSession.Id,
                update
            );

            return new Response<ChatEnhancementResponseDto>
            {
                Status = true,
                Message = "Enhancement completed successfully",
                Data = new ChatEnhancementResponseDto
                {
                    ChatId = chatSession.Id,
                    UserMessage = request.Message,
                    AssistantMessage = aiResponse,
                    CurrentResume = enhancedResume ?? originalResume,
                    EnhancedHtml = enhancedHtml,
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                    Timestamp = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            return new Response<ChatEnhancementResponseDto>
            {
                Status = false,
                Message = $"Failed to process chat enhancement: {ex.Message}",
                Data = null!
            };
        }
    }

    public async Task<Response<List<ChatSessionSummaryDto>>> GetUserChatSessionsAsync(long userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var skip = (page - 1) * pageSize;
            var filter = Builders<ChatSession>.Filter.Eq(c => c.UserId, userId);
            var sort = Builders<ChatSession>.Sort.Descending(c => c.UpdatedAt);

            var sessions = await _chatSessionsCollection
                .Find(filter)
                .Sort(sort)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            var summaries = new List<ChatSessionSummaryDto>();
            
            foreach (var session in sessions)
            {
                // Count messages from history collection
                var messageCount = await _historyCollection.CountDocumentsAsync(
                    Builders<ResumeEnhancementHistory>.Filter.And(
                        Builders<ResumeEnhancementHistory>.Filter.Eq(h => h.ChatId, session.Id),
                        Builders<ResumeEnhancementHistory>.Filter.Eq(h => h.UserId, userId)
                    )
                );
                
                summaries.Add(new ChatSessionSummaryDto
                {
                    ChatId = session.Id,
                    Title = session.Title,
                    CreatedAt = session.CreatedAt,
                    UpdatedAt = session.UpdatedAt,
                    MessageCount = (int)messageCount,
                    IsActive = session.IsActive
                });
            }

            return new Response<List<ChatSessionSummaryDto>>
            {
                Status = true,
                Message = $"Retrieved {summaries.Count} chat sessions",
                Data = summaries
            };
        }
        catch (Exception ex)
        {
            return new Response<List<ChatSessionSummaryDto>>
            {
                Status = false,
                Message = $"Failed to retrieve chat sessions: {ex.Message}",
                Data = new List<ChatSessionSummaryDto>()
            };
        }
    }

    public async Task<Response<ChatSessionDetailDto>> GetChatSessionByIdAsync(long userId, string chatId)
    {
        try
        {
            var filter = Builders<ChatSession>.Filter.And(
                Builders<ChatSession>.Filter.Eq(c => c.Id, chatId),
                Builders<ChatSession>.Filter.Eq(c => c.UserId, userId)
            );

            var session = await _chatSessionsCollection.Find(filter).FirstOrDefaultAsync();

            if (session == null)
            {
                return new Response<ChatSessionDetailDto>
                {
                    Status = false,
                    Message = "Chat session not found",
                    Data = null!
                };
            }

            // Fetch messages from history collection
            var historyFilter = Builders<ResumeEnhancementHistory>.Filter.And(
                Builders<ResumeEnhancementHistory>.Filter.Eq(h => h.ChatId, chatId),
                Builders<ResumeEnhancementHistory>.Filter.Eq(h => h.UserId, userId)
            );
            
            var historyEntries = await _historyCollection
                .Find(historyFilter)
                .SortBy(h => h.CreatedAt)
                .ToListAsync();

            // Convert history entries to chat messages (each entry becomes 2 messages: user + assistant)
            var messages = new List<ChatMessageDto>();
            foreach (var entry in historyEntries)
            {
                // Add user message
                messages.Add(new ChatMessageDto
                {
                    Id = entry.Id + "_user",
                    Role = "user",
                    Content = entry.Message ?? entry.UserMessage ?? "",
                    ResumeData = entry.OriginalResume != null ? ConvertToResumeDto(entry.OriginalResume) : null,
                    Timestamp = entry.CreatedAt,
                    ProcessingTimeMs = null
                });
                
                // Add assistant message
                messages.Add(new ChatMessageDto
                {
                    Id = entry.Id + "_assistant",
                    Role = "assistant",
                    Content = entry.AssistantMessage ?? "",
                    ResumeData = entry.EnhancedResume != null ? ConvertToResumeDto(entry.EnhancedResume) : null,
                    Timestamp = entry.CreatedAt,
                    ProcessingTimeMs = entry.ProcessingTimeMs
                });
            }

            // Get current resume from latest enhanced resume
            var latestResumeEntry = historyEntries
                .Where(h => h.EnhancedResume != null)
                .OrderByDescending(h => h.CreatedAt)
                .FirstOrDefault();
            
            var currentResume = latestResumeEntry?.EnhancedResume != null 
                ? ConvertToResumeDto(latestResumeEntry.EnhancedResume) 
                : null;

            var detail = new ChatSessionDetailDto
            {
                ChatId = session.Id,
                Title = session.Title,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt,
                Messages = messages,
                CurrentResume = currentResume,
                IsActive = session.IsActive
            };

            return new Response<ChatSessionDetailDto>
            {
                Status = true,
                Message = "Chat session retrieved successfully",
                Data = detail
            };
        }
        catch (Exception ex)
        {
            return new Response<ChatSessionDetailDto>
            {
                Status = false,
                Message = $"Failed to retrieve chat session: {ex.Message}",
                Data = null!
            };
        }
    }

    public async Task<Response<List<EnhancementHistorySummaryDto>>> GetChatHistorySummaryAsync(long userId, string chatId, int page = 1, int pageSize = 20, string sortOrder = "desc", string search = "", string? templateId = null)
    {
        try
        {
            var builder = Builders<ResumeEnhancementHistory>.Filter;
            var filter = builder.And(
                builder.Eq(h => h.ChatId, chatId),
                builder.Eq(h => h.UserId, userId)
            );

            if (!string.IsNullOrWhiteSpace(templateId))
            {
                filter = builder.And(filter, builder.Eq(h => h.TemplateId, templateId));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchFilter = builder.Or(
                    builder.Regex(h => h.Message, new BsonRegularExpression(search, "i")),
                    builder.Regex(h => h.UserMessage, new BsonRegularExpression(search, "i"))
                );
                filter = builder.And(filter, searchFilter);
            }

            // Fetch history sorted by creation date
            var sort = sortOrder.ToLower() == "asc" 
                ? Builders<ResumeEnhancementHistory>.Sort.Ascending(h => h.CreatedAt)
                : Builders<ResumeEnhancementHistory>.Sort.Descending(h => h.CreatedAt);
            
            var skip = (page - 1) * pageSize;

            var historyEntries = await _historyCollection
                .Find(filter)
                .Sort(sort)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();

            var dtos = historyEntries.Select(h => new EnhancementHistorySummaryDto
            {
                Id = h.Id,
                // Use Message field, fallback to UserMessage for legacy data
                UserMessage = h.Message ?? h.UserMessage ?? "",
                TemplateId = h.TemplateId,
                CreatedAt = h.CreatedAt
            }).ToList();

            return new Response<List<EnhancementHistorySummaryDto>>
            {
                Status = true,
                Message = "History retrieved successfully",
                Data = dtos
            };
        }
        catch (Exception ex)
        {
            return new Response<List<EnhancementHistorySummaryDto>>
            {
                Status = false,
                Message = $"Failed to retrieve history: {ex.Message}",
                Data = null!
            };
        }
    }

    public async Task<Response<EnhancementHistoryDetailDto>> GetEnhancementHistoryDetailAsync(long userId, string historyId)
    {
        try
        {
            var filter = Builders<ResumeEnhancementHistory>.Filter.And(
                Builders<ResumeEnhancementHistory>.Filter.Eq(h => h.Id, historyId),
                Builders<ResumeEnhancementHistory>.Filter.Eq(h => h.UserId, userId)
            );

            var historyEntry = await _historyCollection.Find(filter).FirstOrDefaultAsync();

            if (historyEntry == null)
            {
                return new Response<EnhancementHistoryDetailDto>
                {
                    Status = false,
                    Message = "History entry not found",
                    Data = null!
                };
            }

            var dto = new EnhancementHistoryDetailDto
            {
                Id = historyEntry.Id,
                ChatId = historyEntry.ChatId ?? string.Empty,
                TemplateId = historyEntry.TemplateId,
                UserMessage = historyEntry.Message ?? historyEntry.UserMessage ?? "",
                AssistantMessage = historyEntry.AssistantMessage,
                OriginalResume = historyEntry.OriginalResume != null ? ConvertToResumeDto(historyEntry.OriginalResume) : null,
                EnhancedResume = historyEntry.EnhancedResume != null ? ConvertToResumeDto(historyEntry.EnhancedResume) : null,
                ResumeHtml = historyEntry.ResumeHtml,
                EnhancedHtml = historyEntry.EnhancedHtml,
                CreatedAt = historyEntry.CreatedAt,
                ProcessingTimeMs = historyEntry.ProcessingTimeMs
            };

            return new Response<EnhancementHistoryDetailDto>
            {
                Status = true,
                Message = "History retrieved successfully",
                Data = dto
            };
        }
        catch (Exception ex)
        {
            return new Response<EnhancementHistoryDetailDto>
            {
                Status = false,
                Message = $"Failed to retrieve history detail: {ex.Message}",
                Data = null!
            };
        }
    }

    public async Task<Response<bool>> DeleteChatSessionAsync(long userId, string chatId)
    {
        try
        {
            var filter = Builders<ChatSession>.Filter.And(
                Builders<ChatSession>.Filter.Eq(c => c.Id, chatId),
                Builders<ChatSession>.Filter.Eq(c => c.UserId, userId)
            );

            var result = await _chatSessionsCollection.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
            {
                return new Response<bool>
                {
                    Status = false,
                    Message = "Chat session not found",
                    Data = false
                };
            }

            return new Response<bool>
            {
                Status = true,
                Message = "Chat session deleted successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new Response<bool>
            {
                Status = false,
                Message = $"Failed to delete chat session: {ex.Message}",
                Data = false
            };
        }
    }

    public async Task<Response<ChatSessionSummaryDto>> UpdateChatTitleAsync(long userId, string chatId, string newTitle)
    {
        try
        {
            var filter = Builders<ChatSession>.Filter.And(
                Builders<ChatSession>.Filter.Eq(c => c.Id, chatId),
                Builders<ChatSession>.Filter.Eq(c => c.UserId, userId)
            );

            var update = Builders<ChatSession>.Update
                .Set(c => c.Title, newTitle)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            var session = await _chatSessionsCollection.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<ChatSession> { ReturnDocument = ReturnDocument.After }
            );

            if (session == null)
            {
                return new Response<ChatSessionSummaryDto>
                {
                    Status = false,
                    Message = "Chat session not found",
                    Data = null!
                };
            }

            return new Response<ChatSessionSummaryDto>
            {
                Status = true,
                Message = "Chat title updated successfully",
                Data = new ChatSessionSummaryDto
                {
                    ChatId = session.Id,
                    Title = session.Title,
                    CreatedAt = session.CreatedAt,
                    UpdatedAt = session.UpdatedAt,
                    MessageCount = (int)await _historyCollection.CountDocumentsAsync(
                    Builders<ResumeEnhancementHistory>.Filter.And(
                        Builders<ResumeEnhancementHistory>.Filter.Eq(h => h.ChatId, session.Id),
                        Builders<ResumeEnhancementHistory>.Filter.Eq(h => h.UserId, userId)
                    )
                ),
                    IsActive = session.IsActive
                }
            };
        }
        catch (Exception ex)
        {
            return new Response<ChatSessionSummaryDto>
            {
                Status = false,
                Message = $"Failed to update chat title: {ex.Message}",
                Data = null!
            };
        }
    }

    #endregion

    #region Private Helper Methods (Chat)

    private string BuildConversationContextFromHistory(List<ResumeEnhancementHistory> history, string currentMessage, ResumeDto? currentResume)
    {
        var context = new System.Text.StringBuilder();
        
        if (currentResume != null)
        {
            context.AppendLine("Current Resume Context:");
            context.AppendLine(JsonSerializer.Serialize(currentResume, new JsonSerializerOptions { WriteIndented = true }));
            context.AppendLine();
        }

        context.AppendLine("Conversation History:");
        foreach (var entry in history.TakeLast(10)) // Last 10 messages for context
        {
            // Use Message field (new) with fallback to UserMessage (legacy)
            var userMsg = entry.Message ?? entry.UserMessage ?? "";
            if (!string.IsNullOrEmpty(userMsg))
            {
                context.AppendLine($"user: {userMsg}");
            }
            if (!string.IsNullOrEmpty(entry.AssistantMessage))
            {
                context.AppendLine($"assistant: {entry.AssistantMessage}");
            }
        }
        
        // Add current user message
        context.AppendLine($"user: {currentMessage}");

        return context.ToString();
    }

    private async Task<string> GetConversationalResponseAsync(string context)
    {
        // For now, return a helpful message
        // In future, you could call Ollama for conversational AI
        return "I'm ready to help you enhance your resume. Please provide your resume data or ask me any questions about resume writing!";
    }

    private string GenerateChatTitle(string firstMessage)
    {
        // Generate a title from the first message (max 50 chars)
        var title = firstMessage.Length > 50 
            ? firstMessage.Substring(0, 47) + "..." 
            : firstMessage;
        return title;
    }

    #endregion

    #endregion
}
