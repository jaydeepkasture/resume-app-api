# Chat-Based Resume Enhancement API

## Overview

The Resume Enhancement API now supports **ChatGPT-like conversational enhancement** where users can have ongoing conversations about their resume without needing to send the complete resume data with every request.

## Key Features

✅ **Optional Resume Data** - Users can chat without providing full resume initially  
✅ **Conversation History** - All messages stored in chat sessions  
✅ **Chat Sessions** - Like ChatGPT, each conversation has its own session  
✅ **Auto-Generated Titles** - Chat titles generated from first message  
✅ **Resume State Tracking** - Current resume state maintained across messages  
✅ **Backward Compatible** - Legacy endpoints still work  

## Architecture

### Chat Session Flow
```
1. User creates chat session (optional)
   ↓
2. User sends message with optional resume data
   ↓
3. System creates/updates chat session
   ↓
4. AI processes message with conversation context
   ↓
5. Response stored in chat history
   ↓
6. User continues conversation in same session
```

### Data Models

#### ChatSession
```json
{
  "id": "mongodb_object_id",
  "user_id": 123,
  "title": "Enhance my software engineer resume",
  "created_at": "2026-01-20T10:00:00Z",
  "updated_at": "2026-01-20T10:15:00Z",
  "messages": [...],
  "current_resume": {...},
  "is_active": true
}
```

#### ChatMessage
```json
{
  "id": "guid",
  "role": "user" | "assistant",
  "content": "Make my experience section more impactful",
  "resume_data": {...},
  "timestamp": "2026-01-20T10:00:00Z",
  "processing_time_ms": 3500
}
```

## API Endpoints

### 1. Create Chat Session
**POST** `/api/resume/chat/create`

Creates a new chat session for resume enhancement.

**Request:**
```json
{
  "title": "My Resume Enhancement Chat",  // Optional
  "initialResume": {                      // Optional
    "name": "John Doe",
    "email": "john@example.com",
    // ... other resume fields
  }
}
```

**Response:**
```json
{
  "status": true,
  "message": "Chat session created successfully",
  "data": {
    "chatId": "65f8a1b2c3d4e5f6a7b8c9d0",
    "title": "My Resume Enhancement Chat",
    "createdAt": "2026-01-20T10:00:00Z",
    "updatedAt": "2026-01-20T10:00:00Z",
    "messageCount": 0,
    "isActive": true
  }
}
```

### 2. Chat Enhance (Main Endpoint)
**POST** `/api/resume/chat/enhance`

Send a message in a chat session. Resume data is optional!

**Request (New Chat):**
```json
{
  "chatId": null,  // Creates new chat if null
  "message": "I want to enhance my resume for a senior developer role",
  "resumeData": {  // Optional - can be null
    "name": "John Doe",
    "experience": [...]
  }
}
```

**Request (Existing Chat):**
```json
{
  "chatId": "65f8a1b2c3d4e5f6a7b8c9d0",
  "message": "Make the experience section more quantifiable",
  "resumeData": null  // Uses resume from chat history
}
```

**Response:**
```json
{
  "status": true,
  "message": "Enhancement completed successfully",
  "data": {
    "chatId": "65f8a1b2c3d4e5f6a7b8c9d0",
    "userMessage": "Make the experience section more quantifiable",
    "assistantMessage": "I've enhanced your resume...",
    "currentResume": {...},  // Enhanced resume
    "processingTimeMs": 3500,
    "timestamp": "2026-01-20T10:05:00Z"
  }
}
```

### 3. Get Chat Sessions
**GET** `/api/resume/chat/sessions?page=1&pageSize=20`

Get all chat sessions for the current user.

**Response:**
```json
{
  "status": true,
  "message": "Retrieved 5 chat sessions",
  "data": [
    {
      "chatId": "65f8a1b2c3d4e5f6a7b8c9d0",
      "title": "Enhance my software engineer resume",
      "createdAt": "2026-01-20T10:00:00Z",
      "updatedAt": "2026-01-20T10:15:00Z",
      "messageCount": 6,
      "isActive": true
    }
  ]
}
```

### 4. Get Chat Session Detail
**GET** `/api/resume/chat/{chatId}`

Get full chat session with all messages.

**Response:**
```json
{
  "status": true,
  "message": "Chat session retrieved successfully",
  "data": {
    "chatId": "65f8a1b2c3d4e5f6a7b8c9d0",
    "title": "Enhance my software engineer resume",
    "createdAt": "2026-01-20T10:00:00Z",
    "updatedAt": "2026-01-20T10:15:00Z",
    "messages": [
      {
        "id": "msg-1",
        "role": "user",
        "content": "I want to enhance my resume",
        "resumeData": {...},
        "timestamp": "2026-01-20T10:00:00Z"
      },
      {
        "id": "msg-2",
        "role": "assistant",
        "content": "I've enhanced your resume...",
        "resumeData": {...},
        "timestamp": "2026-01-20T10:00:30Z",
        "processingTimeMs": 3500
      }
    ],
    "currentResume": {...},
    "isActive": true
  }
}
```

### 5. Delete Chat Session
**DELETE** `/api/resume/chat/{chatId}`

Delete a chat session.

**Response:**
```json
{
  "status": true,
  "message": "Chat session deleted successfully",
  "data": true
}
```

### 6. Update Chat Title
**PATCH** `/api/resume/chat/{chatId}/title`

Update the title of a chat session.

**Request:**
```json
{
  "title": "Senior Developer Resume Enhancement"
}
```

**Response:**
```json
{
  "status": true,
  "message": "Chat title updated successfully",
  "data": {
    "chatId": "65f8a1b2c3d4e5f6a7b8c9d0",
    "title": "Senior Developer Resume Enhancement",
    "createdAt": "2026-01-20T10:00:00Z",
    "updatedAt": "2026-01-20T10:20:00Z",
    "messageCount": 6,
    "isActive": true
  }
}
```

## Usage Examples

### Scenario 1: Start Fresh (No Resume Yet)
```javascript
// Step 1: Just start chatting
POST /api/resume/chat/enhance
{
  "chatId": null,
  "message": "I need help creating a resume for a software engineer position",
  "resumeData": null
}

// Response: AI provides guidance
{
  "chatId": "new-chat-id",
  "assistantMessage": "I'm ready to help you enhance your resume..."
}
```

### Scenario 2: Provide Resume and Iterate
```javascript
// Step 1: Send initial resume
POST /api/resume/chat/enhance
{
  "chatId": null,
  "message": "Please review my resume",
  "resumeData": { /* full resume */ }
}

// Step 2: Continue in same chat
POST /api/resume/chat/enhance
{
  "chatId": "chat-id-from-step-1",
  "message": "Make the experience section more impactful",
  "resumeData": null  // Uses resume from chat history
}

// Step 3: Further refinement
POST /api/resume/chat/enhance
{
  "chatId": "chat-id-from-step-1",
  "message": "Add more technical skills",
  "resumeData": null
}
```

### Scenario 3: Multiple Chats for Different Resumes
```javascript
// Chat 1: Software Engineer Resume
POST /api/resume/chat/create
{
  "title": "Software Engineer Resume",
  "initialResume": { /* resume 1 */ }
}

// Chat 2: Data Scientist Resume
POST /api/resume/chat/create
{
  "title": "Data Scientist Resume",
  "initialResume": { /* resume 2 */ }
}

// Work on each independently
POST /api/resume/chat/enhance
{
  "chatId": "chat-1-id",
  "message": "Enhance for backend role"
}

POST /api/resume/chat/enhance
{
  "chatId": "chat-2-id",
  "message": "Emphasize ML experience"
}
```

## Comparison: Legacy vs Chat-Based

### Legacy Endpoint
```javascript
POST /api/resume/enhance
{
  "resumeData": { /* REQUIRED - full resume */ },
  "enhancementInstruction": "Make it better"
}
```

**Limitations:**
- ❌ Must send full resume every time
- ❌ No conversation history
- ❌ No context from previous requests
- ❌ One-shot enhancement only

### Chat-Based Endpoint
```javascript
POST /api/resume/chat/enhance
{
  "chatId": "optional-chat-id",
  "message": "Make it better",
  "resumeData": null  // OPTIONAL
}
```

**Advantages:**
- ✅ Resume data optional
- ✅ Full conversation history
- ✅ Context-aware enhancements
- ✅ Iterative improvements
- ✅ Multiple chat sessions
- ✅ Auto-generated titles

## MongoDB Collections

### chat_sessions
```javascript
{
  "_id": ObjectId("..."),
  "user_id": 123,
  "title": "Enhance my resume",
  "created_at": ISODate("..."),
  "updated_at": ISODate("..."),
  "messages": [
    {
      "id": "guid",
      "role": "user",
      "content": "...",
      "resume_data": {...},
      "timestamp": ISODate("...")
    }
  ],
  "current_resume": {...},
  "is_active": true
}
```

**Indexes:**
- `user_id` (ascending)
- `user_id` + `updated_at` (compound, for pagination)

## Best Practices

### 1. Start with Chat Creation (Optional but Recommended)
```javascript
// Create chat with title for better organization
POST /api/resume/chat/create
{
  "title": "Senior Backend Developer Resume"
}
```

### 2. Send Resume Once, Iterate Many Times
```javascript
// First message: send resume
POST /api/resume/chat/enhance
{
  "chatId": "chat-id",
  "message": "Review my resume",
  "resumeData": { /* full resume */ }
}

// Subsequent messages: no resume needed
POST /api/resume/chat/enhance
{
  "chatId": "chat-id",
  "message": "Make experience more quantifiable"
}
```

### 3. Use Descriptive Messages
```javascript
// ✅ Good
"Add quantifiable achievements to my experience section"

// ❌ Too vague
"Make it better"
```

### 4. Organize with Multiple Chats
```javascript
// Different chats for different purposes
- "Software Engineer Resume - Google"
- "Software Engineer Resume - Startup"
- "Data Scientist Resume"
```

## Frontend Integration Example

```typescript
// Create a new chat
const createChat = async () => {
  const response = await fetch('/api/resume/chat/create', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      title: 'New Resume Enhancement'
    })
  });
  const data = await response.json();
  return data.data.chatId;
};

// Send message in chat
const sendMessage = async (chatId, message, resumeData = null) => {
  const response = await fetch('/api/resume/chat/enhance', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      chatId,
      message,
      resumeData
    })
  });
  return await response.json();
};

// Get all chats
const getChats = async () => {
  const response = await fetch('/api/resume/chat/sessions', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  return await response.json();
};

// Get chat history
const getChatHistory = async (chatId) => {
  const response = await fetch(`/api/resume/chat/${chatId}`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  return await response.json();
};
```

## Migration from Legacy API

### Before (Legacy)
```javascript
// Every request needs full resume
await fetch('/api/resume/enhance', {
  method: 'POST',
  body: JSON.stringify({
    resumeData: fullResume,
    enhancementInstruction: "Make it better"
  })
});
```

### After (Chat-Based)
```javascript
// First time: send resume
const { data } = await fetch('/api/resume/chat/enhance', {
  method: 'POST',
  body: JSON.stringify({
    chatId: null,
    message: "Review my resume",
    resumeData: fullResume
  })
}).then(r => r.json());

const chatId = data.chatId;

// Subsequent requests: just send messages
await fetch('/api/resume/chat/enhance', {
  method: 'POST',
  body: JSON.stringify({
    chatId: chatId,
    message: "Make experience more impactful",
    resumeData: null  // No need to send again!
  })
});
```

## Performance Considerations

- **First Message**: 3-15 seconds (Ollama processing)
- **Subsequent Messages**: 3-15 seconds (with conversation context)
- **Get Chat Sessions**: < 200ms
- **Get Chat Detail**: < 300ms

## Error Handling

```javascript
{
  "status": false,
  "message": "Chat session not found",
  "data": null
}
```

Common errors:
- `Chat session not found` - Invalid chatId
- `Validation failed` - Missing required fields
- `User not authenticated` - Invalid/expired JWT token

---

**The chat-based API provides a much better user experience, similar to ChatGPT, where users can have natural conversations about their resume without repeatedly sending the same data!** 🎉
