# Enhancement History Pagination API

## Date: 2026-01-22

## Overview
Added a new API endpoint to retrieve a paginated summary of enhancement history for a specific chat. This allows listing the history of changes/interactions within a chat session.

## New API Endpoint

**GET** `/api/resume/chat/{chatId}/history`

### Parameters:
- `chatId` (string, path): The ID of the chat session.
- `page` (int, query): Page number (default: 1).
- `pageSize` (int, query): Number of items per page (default: 20, max: 50).

### Response Body:
```json
{
  "status": true,
  "message": "History retrieved successfully",
  "data": [
    {
      "id": "6790...",
      "userMessage": "Add more skills",
      "createdAt": "2026-01-22T12:00:00Z"
    },
    {
      "id": "6790...",
      "userMessage": "Initial resume created",
      "createdAt": "2026-01-22T11:55:00Z"
    }
  ]
}
```

## Changes Made

### 1. **New DTO** (`ResumeInOneMinute.Domain/DTO/EnhancementHistorySummaryDto.cs`)
```csharp
public class EnhancementHistorySummaryDto
{
    public string Id { get; set; }
    public string UserMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 2. **Interface Update** (`IResumeRepository.cs`)
Added `GetChatHistorySummaryAsync` method signature.

### 3. **Repository Implementation** (`ResumeRepository.cs`)
Implemented `GetChatHistorySummaryAsync`:
- Filters by `ChatId` and `UserId`.
- Sorts by `CreatedAt` descending (newest first).
- Implements pagination using `Skip` and `Limit`.
- Handling mapping of legacy vs new message fields: `UserMessage = h.Message ?? h.UserMessage ?? ""`

### 4. **Controller Update** (`ResumeController.cs`)
Added `GetChatHistorySummary` action method.

## Verification
- Build Successful.
- Application Running.
- Endpoint ready to be consumed.
