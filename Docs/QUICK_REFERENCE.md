# Quick Reference: New Data Storage Structure

## Collections

### `chat_sessions` - Metadata Only
```javascript
{
  _id: ObjectId("..."),
  user_id: 123,
  title: "Enhance my resume",
  created_at: ISODate("2026-01-22T10:00:00Z"),
  updated_at: ISODate("2026-01-22T10:30:00Z"),
  is_active: true
}
```

### `resume_enhancement_history` - Messages & Data
```javascript
// User message
{
  _id: ObjectId("..."),
  user_id: 123,
  chat_id: ObjectId("..."),
  role: "user",
  message: "Make my experience section better",
  resume_data: { name: "...", email: "...", ... },
  created_at: ISODate("2026-01-22T10:15:00Z")
}

// Assistant response
{
  _id: ObjectId("..."),
  user_id: 123,
  chat_id: ObjectId("..."),
  role: "assistant",
  message: "I've enhanced your resume...",
  resume_data: { name: "...", email: "...", ... },
  created_at: ISODate("2026-01-22T10:15:30Z"),
  processing_time_ms: 5000
}

// Legacy format (still supported)
{
  _id: ObjectId("..."),
  user_id: 123,
  original_resume: { ... },
  enhanced_resume: { ... },
  enhancement_instruction: "...",
  created_at: ISODate("2026-01-22T10:00:00Z"),
  processing_time_ms: 3000
}
```

## Common Queries

### Get all messages for a chat
```javascript
db.resume_enhancement_history.find({
  chat_id: ObjectId("..."),
  user_id: 123
}).sort({ created_at: 1 })
```

### Get current resume for a chat
```javascript
db.resume_enhancement_history.find({
  chat_id: ObjectId("..."),
  user_id: 123,
  resume_data: { $ne: null }
}).sort({ created_at: -1 }).limit(1)
```

### Count messages in a chat
```javascript
db.resume_enhancement_history.countDocuments({
  chat_id: ObjectId("..."),
  user_id: 123
})
```

### Get user's chat sessions
```javascript
db.chat_sessions.find({
  user_id: 123
}).sort({ updated_at: -1 })
```

## API Endpoints (Unchanged)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/resume/chat/create` | Create new chat session |
| POST | `/api/resume/chat/enhance` | Send message in chat |
| GET | `/api/resume/chat/sessions` | List user's chats |
| GET | `/api/resume/chat/{chatId}` | Get chat with messages |
| DELETE | `/api/resume/chat/{chatId}` | Delete chat |
| PATCH | `/api/resume/chat/{chatId}/title` | Update chat title |
| POST | `/api/resume/enhance` | Legacy enhancement (still works) |
| GET | `/api/resume/history` | Legacy history (still works) |

## Data Flow

### Creating a Chat
1. POST `/api/resume/chat/create` with optional `InitialResume`
2. Creates entry in `chat_sessions` (metadata only)
3. If `InitialResume` provided, creates entry in `resume_enhancement_history`

### Sending a Message
1. POST `/api/resume/chat/enhance` with `ChatId`, `Message`, optional `ResumeData`
2. Fetches chat history from `resume_enhancement_history`
3. Saves user message to `resume_enhancement_history`
4. Calls Ollama API for enhancement
5. Saves assistant response to `resume_enhancement_history`
6. Updates `chat_sessions` metadata (title, updated_at)

### Retrieving a Chat
1. GET `/api/resume/chat/{chatId}`
2. Fetches chat metadata from `chat_sessions`
3. Fetches all messages from `resume_enhancement_history` where `chat_id` matches
4. Determines current resume from latest history entry with `resume_data`
5. Returns combined data

## Indexes

### resume_enhancement_history
- `{ user_id: 1 }` - User's history
- `{ user_id: 1, created_at: -1 }` - User's history sorted
- `{ chat_id: 1, user_id: 1 }` - Chat messages
- `{ chat_id: 1, created_at: 1 }` - Chat messages sorted

### chat_sessions
- `{ user_id: 1 }` - User's chats
- `{ user_id: 1, updated_at: -1 }` - User's chats sorted

## Migration (If Needed)

If you have old chat sessions with embedded messages:

```csharp
// See Scripts/MigrateChatSessionsScript.cs
var migrator = new MigrateChatSessionsScript(
    chatSessionsCollection, 
    historyCollection
);
await migrator.MigrateUsingBsonAsync();
```

This will:
1. Find all chat sessions with embedded `messages` array
2. Create `resume_enhancement_history` entry for each message
3. Remove `messages` and `current_resume` from chat session
