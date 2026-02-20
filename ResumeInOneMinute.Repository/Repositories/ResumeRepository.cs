using MongoDB.Bson;
using MongoDB.Driver;
using ResumeInOneMinute.Domain.Constance;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using ResumeInOneMinute.Domain.Model;
using System.Diagnostics;
using System.Text.Json;
using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Packaging;

namespace ResumeInOneMinute.Repository.Repositories;

public class ResumeRepository : IResumeRepository
{
    private readonly IMongoCollection<ResumeEnhancementHistory> _historyCollection;
    private readonly IMongoCollection<ChatSession> _chatSessionsCollection;
    private readonly IMongoCollection<HtmlTemplate> _htmlTemplateCollection;
    private readonly IMongoCollection<Resume> _resumeCollection;
    private readonly IOllamaService _ollamaService;
    private readonly IGroqService _groqService;

    public ResumeRepository(IMongoDbService mongoDbService, IOllamaService ollamaService, IGroqService groqService)
    {
        _historyCollection = mongoDbService.GetCollection<ResumeEnhancementHistory>(MongoCollections.ResumeEnhancementHistory);
        _chatSessionsCollection = mongoDbService.GetCollection<ChatSession>(MongoCollections.ChatSessions);
        _htmlTemplateCollection = mongoDbService.GetCollection<HtmlTemplate>(MongoCollections.HtmlTemplates);
        _resumeCollection = mongoDbService.GetCollection<Resume>(MongoCollections.Resume);
        _ollamaService = ollamaService;
        _groqService = groqService;
        
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
            var userMessage = ex.Message.Contains("temporarily unavailable") 
                ? ex.Message 
                : $"Failed to enhance resume: {ex.Message}";

            return new Response<ResumeEnhancementResponseDto>
            {
                Status = false,
                Message = userMessage,
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
                EnhancementInstruction = h.EnhancementInstruction ?? h.Message,
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
                EnhancementInstruction = history.EnhancementInstruction ?? history.Message,
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

    private ResumeDto ConvertToResumeDto(BsonDocument? bsonDocument)
    {
        if (bsonDocument == null) return new ResumeDto();
        var json = bsonDocument.ToJson();
        return JsonSerializer.Deserialize<ResumeDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new ResumeDto();
    }

    #region Chat-Based Enhancement Methods

    public async Task<Response<ChatSessionSummaryDto>> CreateChatSessionAsync(long userId, CreateChatSessionDto request, ResumeDto initialResume, bool isEmptyTemplate = false)
    {
        try
        {
            // Determine Chat Title from Template Name
            string chatTitle = "New Resume Chat";
            if (!string.IsNullOrWhiteSpace(request.TemplateId))
            {
                var template = await _htmlTemplateCollection
                    .Find(Builders<HtmlTemplate>.Filter.Eq(t => t.Id, request.TemplateId))
                    .FirstOrDefaultAsync();
                    
                if (template != null)
                {
                    chatTitle = template.TemplateName;
                }
            }
            
            // Create lightweight chat session (metadata only)
            var chatSession = new ChatSession
            {
                UserId = userId,
                Title = chatTitle,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                TemplateId = request.TemplateId,
                IsTitleUpdated = false
            };

            await _chatSessionsCollection.InsertOneAsync(chatSession);

            // Fetch master resume if exists to merge with basic info - Skip if isEmptyTemplate is requested
            if (!isEmptyTemplate)
            {
                var masterResume = await _resumeCollection.Find(r => r.UserId == userId).FirstOrDefaultAsync();
                if (masterResume?.ResumeData != null)
                {
                    var mergedResume = masterResume.ResumeData;
                    // Override with current account info from controller
                    mergedResume.Name = initialResume.Name;
                    mergedResume.Email = initialResume.Email;
                    if (!string.IsNullOrWhiteSpace(initialResume.PhoneNo))
                    {
                        mergedResume.PhoneNo = initialResume.PhoneNo;
                    }
                    initialResume = mergedResume;
                }
            }

            // If initial resume is provided, create the first history entry
            if (initialResume != null)
            {
                var initialHistory = new ResumeEnhancementHistory
                {
                    UserId = userId,
                    ChatId = chatSession.Id,
                    Role = "system",
                    Message = "Initial resume created",
                    OriginalResume = ConvertToBsonDocument(initialResume),
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
                    MessageCount = initialResume != null ? 1 : 0,
                    IsActive = chatSession.IsActive,
                    ResumeData = initialResume
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
                    Builders<ChatSession>.Filter.Eq(c => c.UserId, userId),
                    Builders<ChatSession>.Filter.Eq(c => c.IsDeleted, false)
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
                    IsActive = true,
                    IsTitleUpdated = false
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
            
            // Auto-generate title if not yet updated and message is present
            if (!chatSession.IsTitleUpdated && !string.IsNullOrWhiteSpace(request.Message))
            {
                chatSession.Title = await _ollamaService.GenerateChatTitleAsync(request.Message);
                chatSession.IsTitleUpdated = true;
            }

            var update = Builders<ChatSession>.Update
                .Set(c => c.UpdatedAt, chatSession.UpdatedAt)
                .Set(c => c.Title, chatSession.Title)
                .Set(c => c.IsTitleUpdated, chatSession.IsTitleUpdated);

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
                    Title = chatSession.Title,
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
            var userMessage = ex.Message.Contains("temporarily unavailable") 
                ? ex.Message 
                : $"Failed to process chat enhancement: {ex.Message}";

            return new Response<ChatEnhancementResponseDto>
            {
                Status = false,
                Message = userMessage,
                Data = null!
            };
        }
    }

    public async Task<Response<List<ChatSessionSummaryDto>>> GetUserChatSessionsAsync(long userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var skip = (page - 1) * pageSize;
            var filter = Builders<ChatSession>.Filter.And(
                Builders<ChatSession>.Filter.Eq(c => c.UserId, userId),
                Builders<ChatSession>.Filter.Eq(c => c.IsDeleted, false)
            );
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
                Builders<ChatSession>.Filter.Eq(c => c.UserId, userId),
                Builders<ChatSession>.Filter.Eq(c => c.IsDeleted, false)
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

            

            // Get current resume from latest enhanced or original resume
            var latestResumeEntry = historyEntries
                .Where(h => h.EnhancedResume != null || h.OriginalResume != null)
                .OrderByDescending(h => h.CreatedAt)
                .FirstOrDefault();
            
            var currentResume = latestResumeEntry?.EnhancedResume != null 
                ? ConvertToResumeDto(latestResumeEntry.EnhancedResume) 
                : (latestResumeEntry?.OriginalResume != null 
                    ? ConvertToResumeDto(latestResumeEntry.OriginalResume) 
                    : null);

            var detail = new ChatSessionDetailDto
            {
                ChatId = session.Id,
                Title = session.Title,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt,
                ResumeData = currentResume,
                IsActive = session.IsActive,
                TemplateId = session.TemplateId
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
            // Verify chat session exists and is not deleted
            var sessionFilter = Builders<ChatSession>.Filter.And(
                Builders<ChatSession>.Filter.Eq(c => c.Id, chatId),
                Builders<ChatSession>.Filter.Eq(c => c.UserId, userId),
                Builders<ChatSession>.Filter.Eq(c => c.IsDeleted, false)
            );
            
            var sessionExists = await _chatSessionsCollection.Find(sessionFilter).AnyAsync();
            if (!sessionExists)
            {
                return new Response<List<EnhancementHistorySummaryDto>>
                {
                    Status = false,
                    Message = "Chat session not found",
                    Data = null!
                };
            }

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
                CreatedAt = h.CreatedAt,
                ResumeData = h.EnhancedResume != null ? ConvertToResumeDto(h.EnhancedResume) : (h.OriginalResume != null ? ConvertToResumeDto(h.OriginalResume) : null)
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

            var update = Builders<ChatSession>.Update
                .Set(c => c.IsDeleted, true)
                .Set(c => c.IsActive, false)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            var result = await _chatSessionsCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
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

    public async Task<Response<bool>> SaveResumeAsync(long userId, string chatId, ResumeDto resume, string templateId)
    {
        try
        {
            var filter = Builders<ResumeEnhancementHistory>.Filter.And(
                Builders<ResumeEnhancementHistory>.Filter.Eq(h => h.UserId, userId),
                Builders<ResumeEnhancementHistory>.Filter.Eq(h => h.ChatId, chatId)
            );
            var sort = Builders<ResumeEnhancementHistory>.Sort.Descending(h => h.CreatedAt);
            
            var latestHistory = await _historyCollection.Find(filter).Sort(sort).FirstOrDefaultAsync();
            
            var incomingBson = ConvertToBsonDocument(resume);
            
            // Check if we should skip saving
            // Condition: Latest record has message "save" AND EnhancedResume content is identical
            if (latestHistory != null && 
                string.Equals(latestHistory.Message, "save", StringComparison.OrdinalIgnoreCase))
            {
                // Compare BsonDocuments
                // Note: Direct comparison of BsonDocuments works for equality if structure is same
                if (latestHistory.EnhancedResume != null && latestHistory.EnhancedResume.Equals(incomingBson))
                {
                    return new Response<bool>
                    {
                        Status = true,
                        Message = "Resume state already saved (no changes)",
                        Data = true
                    };
                }
            }
            
            // Insert new record
            var newHistory = new ResumeEnhancementHistory
            {
                UserId = userId,
                ChatId = chatId,
                Message = "save",
                EnhancedResume = incomingBson,
                TemplateId = templateId,
                CreatedAt = DateTime.UtcNow,
                // OriginalResume is not set as this is a snapshot save
                // processing time is negligible
            };
            
            await _historyCollection.InsertOneAsync(newHistory);
            
            return new Response<bool>
            {
                Status = true,
                Message = "Resume saved successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new Response<bool>
            {
                Status = false,
                Message = $"Failed to save resume: {ex.Message}",
                Data = false
            };
        }
    }

    public async Task<Response<object>> UploadResumeAsync(long userId, Stream fileStream, string extension)
    {
        try
        {
            string text = string.Empty;
            extension = extension.ToLower();

            // 1. Extract text from the file locally
            if (extension == ".pdf")
            {
                using var document = PdfDocument.Open(fileStream);
                text = string.Join(" ", document.GetPages().Select(p => p.Text));
                if (string.IsNullOrWhiteSpace(text)) 
                    throw new Exception("Could not extract text from PDF. The file might be scanned or empty.");
            }
            else if (extension == ".docx" || extension == ".doc")
            {
                using var wordDoc = WordprocessingDocument.Open(fileStream, false);
                text = wordDoc.MainDocumentPart?.Document?.Body?.InnerText ?? "";
                if (string.IsNullOrWhiteSpace(text)) 
                    throw new Exception("Could not extract text from Word document. The file might be empty.");
            }
            else
            {
                return new Response<object> 
                { 
                    Status = false, 
                    Message = "Unsupported file format. Please upload a PDF or Word document (.docx, .doc)." 
                };
            }

            // 2. Ask Groq to convert the extracted text into a ResumeDto object
            var parsedResumeDto = await _groqService.ExtractResumeFromTextAsync(text);
            if (parsedResumeDto == null)
            {
                throw new Exception("AI failed to parse the resume data from the extracted text.");
            }

            // 3. Insert/Update the ResumeDto into the resume collection
            var filter = Builders<Resume>.Filter.Eq(r => r.UserId, userId);
            var update = Builders<Resume>.Update
                .Set(r => r.UserId, userId)
                .Set(r => r.ResumeData, parsedResumeDto)
                .Set(r => r.ParsedFrom, extension.TrimStart('.'))
                .Set(r => r.UpdatedAt, DateTime.UtcNow)
                .SetOnInsert(r => r.ParsedAt, DateTime.UtcNow);

            var options = new FindOneAndUpdateOptions<Resume>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            await _resumeCollection.FindOneAndUpdateAsync(filter, update, options);

            return new Response<object>
            {
                Status = true,
                Message = "Resume processed and saved successfully",
                Data = null!
            };
        }
        catch (Exception ex)
        {
            return new Response<object>
            {
                Status = false,
                Message = $"Processing failed: {ex.Message}",
                Data = null!
            };
        }
    }

    public async Task<Response<ChatSessionSummaryDto>> UpdateChatTitleAsync(long userId, string chatId, string newTitle)
    {
        try
        {
            var filter = Builders<ChatSession>.Filter.And(
                Builders<ChatSession>.Filter.Eq(c => c.Id, chatId),
                Builders<ChatSession>.Filter.Eq(c => c.UserId, userId),
                Builders<ChatSession>.Filter.Eq(c => c.IsDeleted, false)
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

    private Task<string> GetConversationalResponseAsync(string context)
    {
        // For now, return a helpful message
        // In future, you could call Ollama for conversational AI
        return Task.FromResult("I'm ready to help you enhance your resume. Please provide your resume data or ask me any questions about resume writing!");
    }
    public async Task<Resume?> GetMasterResumeAsync(long userId)
    {
        return await _resumeCollection.Find(r => r.UserId == userId).FirstOrDefaultAsync();
    }

    #endregion

    public async Task<int> GetUserChatSessionCountAsync(long userId)
    {
        var filter = Builders<ChatSession>.Filter.And(
            Builders<ChatSession>.Filter.Eq(c => c.UserId, userId),
            Builders<ChatSession>.Filter.Eq(c => c.IsDeleted, false)
        );
        return (int)await _chatSessionsCollection.CountDocumentsAsync(filter);
    }

    public async Task<int> GetDailyTokenUsageAsync(long userId)
    {
        var todayStart = DateTime.UtcNow.Date;
        var filter = Builders<ResumeEnhancementHistory>.Filter.And(
            Builders<ResumeEnhancementHistory>.Filter.Eq(h => h.UserId, userId),
            Builders<ResumeEnhancementHistory>.Filter.Gte(h => h.CreatedAt, todayStart)
        );

        var history = await _historyCollection.Find(filter).ToListAsync();
        
        // Sum characters from Message (new) and EnhancementInstruction (legacy)
        return history.Sum(h => (h.Message?.Length ?? 0) + (h.EnhancementInstruction?.Length ?? 0));
    }

    #endregion
}
