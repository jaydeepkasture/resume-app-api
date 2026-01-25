# Enhancement History Detail API

## Date: 2026-01-22

## Overview
Added a new API endpoint to retrieve the full detailed record of a specific enhancement history entry by its ID. This allows viewing the complete "before and after" resume state, the HTML used, and the conversation details.

## New API Endpoint

**GET** `/api/resume/chat/history/{historyId}`

### Parameters:
- `historyId` (string, path): The MongoDB ObjectId of the enhancement history entry.

### Response Body:
```json
{
  "status": true,
  "message": "History retrieved successfully",
  "data": {
    "id": "6790...",
    "chatId": "6789...",
    "userMessage": "Add more skills",
    "assistantMessage": "Here is the updated resume...",
    "originalResume": { ... },     // Full ResumeDto
    "enhancedResume": { ... },     // Full ResumeDto
    "resumeHtml": "<div>...</div>",
    "createdAt": "2026-01-22T12:00:00Z",
    "processingTimeMs": 1500
  }
}
```

## Changes Made

### 1. **New DTO** (`ResumeInOneMinute.Domain/DTO/EnhancementHistoryDetailDto.cs`)
```csharp
public class EnhancementHistoryDetailDto
{
    public string Id { get; set; }
    public string ChatId { get; set; }
    public string UserMessage { get; set; }
    public string AssistantMessage { get; set; }
    public ResumeDto? OriginalResume { get; set; }
    public ResumeDto? EnhancedResume { get; set; }
    public string? ResumeHtml { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? ProcessingTimeMs { get; set; }
}
```

### 2. **Interface Update** (`IResumeRepository.cs`)
Added `GetEnhancementHistoryDetailAsync` method.

### 3. **Repository Implementation** (`ResumeRepository.cs`)
Implemented `GetEnhancementHistoryDetailAsync`:
- Fetches entry by `Id` and `UserId`.
- Maps all fields including legacy `UserMessage` fallback.
- Converts `BsonDocument` resume data to `ResumeDto`.

### 4. **Controller Update** (`ResumeController.cs`)
Added `GetChatHistoryDetail` action method.

## Verification
- Build Successful.
- Endpoint ready to be consumed.
