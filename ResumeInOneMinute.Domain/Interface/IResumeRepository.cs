using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Model;

namespace ResumeInOneMinute.Domain.Interface;

public interface IResumeRepository
{
    // Legacy enhancement (kept for backward compatibility)
    Task<Response<ResumeEnhancementResponseDto>> EnhanceResumeAsync(long userId, ResumeEnhancementRequestDto request);
    
    Task<Response<List<ResumeEnhancementResponseDto>>> GetUserHistoryAsync(long userId, int page = 1, int pageSize = 10);
    
    Task<Response<ResumeEnhancementResponseDto>> GetHistoryByIdAsync(long userId, string historyId);
    
    // New chat-based enhancement methods
    Task<Response<ChatSessionSummaryDto>> CreateChatSessionAsync(long userId, CreateChatSessionDto request, ResumeDto initialResume);
    
    Task<Response<ChatEnhancementResponseDto>> ChatEnhanceAsync(long userId, ChatEnhancementRequestDto request);
    
    Task<Response<List<ChatSessionSummaryDto>>> GetUserChatSessionsAsync(long userId, int page = 1, int pageSize = 20);
    
    Task<Response<ChatSessionDetailDto>> GetChatSessionByIdAsync(long userId, string chatId);
    
    Task<Response<bool>> DeleteChatSessionAsync(long userId, string chatId);
    
    Task<Response<ChatSessionSummaryDto>> UpdateChatTitleAsync(long userId, string chatId, string newTitle);
    
    Task<Response<List<EnhancementHistorySummaryDto>>> GetChatHistorySummaryAsync(long userId, string chatId, int page = 1, int pageSize = 20, string sortOrder = "desc", string search = "", string? templateId = null);
    
    Task<Response<EnhancementHistoryDetailDto>> GetEnhancementHistoryDetailAsync(long userId, string historyId);
    
    Task<Response<bool>> SaveResumeAsync(long userId, string chatId, ResumeDto resume, string templateId);
    
    Task<Response<object>> UploadResumeAsync(long userId, Stream fileStream, string extension);
}

