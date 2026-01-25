# Enhancement History Pagination API - Sorted

## Date: 2026-01-22

## Overview
Updated the enhancement history summary API to support sorting by creation date.

## Updated API Endpoint

**GET** `/api/resume/chat/{chatId}/history`

### Parameters:
- `chatId` (string, path): The ID of the chat session.
- `page` (int, query): Page number (default: 1).
- `pageSize` (int, query): Number of items per page (default: 20, max: 50).
- `sortOrder` (string, query): Sort order by creation date. Values: "asc" or "desc" (default: "desc").

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
    ...
  ]
}
```

## Changes Made

### 1. **Interface Update** (`IResumeRepository.cs`)
Updated `GetChatHistorySummaryAsync` signature to include `sortOrder` parameter.

### 2. **Repository Implementation** (`ResumeRepository.cs`)
Updated implementation to sort using the `sortOrder` parameter:
```csharp
var sort = sortOrder.ToLower() == "asc" 
    ? Builders<ResumeEnhancementHistory>.Sort.Ascending(h => h.CreatedAt)
    : Builders<ResumeEnhancementHistory>.Sort.Descending(h => h.CreatedAt);
```

### 3. **Controller Update** (`ResumeController.cs`)
Updated `GetChatHistorySummary` endpoint to accept `sortOrder` query parameter.

## Verification
- Build Successful.
- Endpoint supports `sortOrder=asc` and `sortOrder=desc`.
