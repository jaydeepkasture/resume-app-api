# Enhancement History Pagination API - Search Support

## Date: 2026-01-22

## Overview
Updated the enhancement history summary API to support searching messages.

## Updated API Endpoint

**GET** `/api/resume/chat/{chatId}/history`

### Parameters:
- `chatId` (string, path): The ID of the chat session.
- `page` (int, query): Page number (default: 1).
- `pageSize` (int, query): Number of items per page (default: 20, max: 50).
- `sortOrder` (string, query): Sort order by creation date. Values: "asc" or "desc" (default: "desc").
- `search` (string, query): Search term to filter messages by. Case-insensitive regex match on `message` (and legacy `user_message`).

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
Updated `GetChatHistorySummaryAsync` signature to include `search` parameter.

### 2. **Repository Implementation** (`ResumeRepository.cs`)
Updated implementation to filter using regex when `search` is provided.
```csharp
if (!string.IsNullOrWhiteSpace(search))
{
    var searchFilter = builder.Or(
        builder.Regex(h => h.Message, new BsonRegularExpression(search, "i")),
        builder.Regex(h => h.UserMessage, new BsonRegularExpression(search, "i"))
    );
    filter = builder.And(filter, searchFilter);
}
```

### 3. **Controller Update** (`ResumeController.cs`)
Updated `GetChatHistorySummary` endpoint to accept `search` query parameter.

## Verification
- Build Successful.
- Endpoint supports `search` parameter for filtering messages.
