# Migration Visual Guide

## Before Migration

```
┌─────────────────────────────────────────────────────────────┐
│                    chat_sessions Collection                  │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Document 1 (Chat Session)                                   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ _id: "chat123"                                       │   │
│  │ user_id: 1                                           │   │
│  │ title: "Enhance my resume"                           │   │
│  │ created_at: "2026-01-20T10:00:00Z"                   │   │
│  │ updated_at: "2026-01-20T10:30:00Z"                   │   │
│  │                                                       │   │
│  │ messages: [  ⚠️ EMBEDDED ARRAY (PROBLEM!)            │   │
│  │   {                                                   │   │
│  │     id: "msg1",                                       │   │
│  │     role: "user",                                     │   │
│  │     content: "Make my resume better",                │   │
│  │     resume_data: { name: "John", ... },              │   │
│  │     timestamp: "2026-01-20T10:15:00Z"                │   │
│  │   },                                                  │   │
│  │   {                                                   │   │
│  │     id: "msg2",                                       │   │
│  │     role: "assistant",                                │   │
│  │     content: "I've enhanced your resume...",         │   │
│  │     resume_data: { name: "John", ... },              │   │
│  │     timestamp: "2026-01-20T10:15:30Z",               │   │
│  │     processing_time_ms: 5000                         │   │
│  │   }                                                   │   │
│  │ ],                                                    │   │
│  │                                                       │   │
│  │ current_resume: { name: "John", ... }  ⚠️ DUPLICATE  │   │
│  │ is_active: true                                      │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                               │
└─────────────────────────────────────────────────────────────┘

Problems:
❌ Messages embedded in chat session
❌ Document grows with each message
❌ 16MB MongoDB document size limit
❌ Inefficient queries
❌ Duplicate resume data
```

## After Migration

```
┌─────────────────────────────────────────────────────────────┐
│                    chat_sessions Collection                  │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Document 1 (Lightweight Metadata Only)                      │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ _id: "chat123"                                       │   │
│  │ user_id: 1                                           │   │
│  │ title: "Enhance my resume"                           │   │
│  │ created_at: "2026-01-20T10:00:00Z"                   │   │
│  │ updated_at: "2026-01-20T10:30:00Z"                   │   │
│  │ is_active: true                                      │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ References via chat_id
                              ▼
┌─────────────────────────────────────────────────────────────┐
│              resume_enhancement_history Collection           │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  Document 1 (User Message)                                   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ _id: "hist1"                                         │   │
│  │ user_id: 1                                           │   │
│  │ chat_id: "chat123"  ◄─── Links to chat session      │   │
│  │ role: "user"                                         │   │
│  │ message: "Make my resume better"                     │   │
│  │ resume_data: { name: "John", ... }                   │   │
│  │ created_at: "2026-01-20T10:15:00Z"                   │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                               │
│  Document 2 (Assistant Response)                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ _id: "hist2"                                         │   │
│  │ user_id: 1                                           │   │
│  │ chat_id: "chat123"  ◄─── Links to chat session      │   │
│  │ role: "assistant"                                    │   │
│  │ message: "I've enhanced your resume..."              │   │
│  │ resume_data: { name: "John", ... }                   │   │
│  │ created_at: "2026-01-20T10:15:30Z"                   │   │
│  │ processing_time_ms: 5000                             │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                               │
└─────────────────────────────────────────────────────────────┘

Benefits:
✅ Lightweight chat sessions
✅ No document size limits
✅ Efficient queries with indexes
✅ Scalable to unlimited messages
✅ Better data organization
```

## Migration Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    MIGRATION PROCESS                         │
└─────────────────────────────────────────────────────────────┘

Step 1: Find Sessions with Embedded Messages
┌──────────────────────────────────────┐
│ db.chat_sessions.find({              │
│   messages: { $exists: true }        │
│ })                                   │
└──────────────────────────────────────┘
                │
                ▼
Step 2: For Each Session
┌──────────────────────────────────────┐
│ For each message in messages array:  │
│                                       │
│ 1. Extract message data               │
│ 2. Create new history entry           │
│    - Set chat_id = session._id        │
│    - Set user_id = session.user_id    │
│    - Set role = message.role          │
│    - Set message = message.content    │
│    - Set resume_data = message.resume │
│    - Set created_at = message.time    │
│ 3. Insert into history collection     │
└──────────────────────────────────────┘
                │
                ▼
Step 3: Clean Up Chat Session
┌──────────────────────────────────────┐
│ db.chat_sessions.updateOne(          │
│   { _id: session._id },               │
│   {                                   │
│     $unset: {                         │
│       messages: "",                   │
│       current_resume: ""              │
│     }                                 │
│   }                                   │
│ )                                     │
└──────────────────────────────────────┘
                │
                ▼
Step 4: Verification
┌──────────────────────────────────────┐
│ ✅ Chat session has no messages field │
│ ✅ History entries created            │
│ ✅ All data preserved                 │
│ ✅ Links maintained via chat_id       │
└──────────────────────────────────────┘
```

## API Workflow

```
┌─────────────────────────────────────────────────────────────┐
│                    MIGRATION API WORKFLOW                    │
└─────────────────────────────────────────────────────────────┘

1. Check Status
   GET /api/admin/migration/chat-sessions/status
   │
   ├─► Returns: {
   │     sessionsNeedingMigration: 5,
   │     migrationComplete: false
   │   }
   │
   ▼

2. Dry Run (Optional but Recommended)
   GET /api/admin/migration/chat-sessions/dry-run
   │
   ├─► Returns: {
   │     migratedSessions: 5,
   │     migratedMessages: 23,
   │     failedCount: 0
   │   }
   │
   ▼

3. Execute Migration
   POST /api/admin/migration/chat-sessions/execute
   │
   ├─► Processing...
   │   ├─ Find sessions with messages
   │   ├─ Extract and migrate each message
   │   ├─ Clean up chat sessions
   │   └─ Log results
   │
   ├─► Returns: {
   │     status: true,
   │     message: "Migration completed!",
   │     migratedSessions: 5,
   │     migratedMessages: 23
   │   }
   │
   ▼

4. Verify
   GET /api/admin/migration/chat-sessions/status
   │
   └─► Returns: {
         sessionsNeedingMigration: 0,
         migrationComplete: true
       }
```

## Data Retrieval After Migration

```
┌─────────────────────────────────────────────────────────────┐
│              HOW CHAT MESSAGES ARE RETRIEVED                 │
└─────────────────────────────────────────────────────────────┘

GET /api/resume/chat/{chatId}

Step 1: Get Chat Session Metadata
┌──────────────────────────────────────┐
│ db.chat_sessions.findOne({           │
│   _id: chatId,                        │
│   user_id: userId                     │
│ })                                    │
└──────────────────────────────────────┘
                │
                ▼
Step 2: Get Messages from History
┌──────────────────────────────────────┐
│ db.resume_enhancement_history.find({ │
│   chat_id: chatId,                    │
│   user_id: userId                     │
│ }).sort({ created_at: 1 })            │
└──────────────────────────────────────┘
                │
                ▼
Step 3: Get Current Resume
┌──────────────────────────────────────┐
│ Latest message with resume_data:     │
│                                       │
│ db.resume_enhancement_history        │
│   .find({                             │
│     chat_id: chatId,                  │
│     resume_data: { $ne: null }        │
│   })                                  │
│   .sort({ created_at: -1 })           │
│   .limit(1)                           │
└──────────────────────────────────────┘
                │
                ▼
Step 4: Combine and Return
┌──────────────────────────────────────┐
│ {                                     │
│   chatId: "...",                      │
│   title: "...",                       │
│   messages: [...],  ◄─ From history  │
│   currentResume: {...}  ◄─ Latest    │
│ }                                     │
└──────────────────────────────────────┘
```

## Indexes for Performance

```
┌─────────────────────────────────────────────────────────────┐
│                    DATABASE INDEXES                          │
└─────────────────────────────────────────────────────────────┘

resume_enhancement_history:
  ┌──────────────────────────────────────┐
  │ { user_id: 1 }                       │  ◄─ User's history
  │ { user_id: 1, created_at: -1 }       │  ◄─ Recent first
  │ { chat_id: 1, user_id: 1 }           │  ◄─ Chat messages
  │ { chat_id: 1, created_at: 1 }        │  ◄─ Ordered messages
  └──────────────────────────────────────┘

chat_sessions:
  ┌──────────────────────────────────────┐
  │ { user_id: 1 }                       │  ◄─ User's chats
  │ { user_id: 1, updated_at: -1 }       │  ◄─ Recent chats
  └──────────────────────────────────────┘

Result: Fast queries even with millions of messages!
```

## Summary

```
┌─────────────────────────────────────────────────────────────┐
│                    MIGRATION SUMMARY                         │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  BEFORE:  One big document with embedded messages            │
│           ❌ Size limits                                      │
│           ❌ Slow queries                                     │
│           ❌ Inefficient                                      │
│                                                               │
│  AFTER:   Separated metadata and messages                    │
│           ✅ No size limits                                   │
│           ✅ Fast indexed queries                             │
│           ✅ Scalable                                         │
│           ✅ Better organized                                 │
│                                                               │
│  MIGRATION: Safe, idempotent, with dry-run                   │
│             ✅ Preview before execution                       │
│             ✅ Can retry if needed                            │
│             ✅ Detailed logging                               │
│             ✅ Error handling                                 │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```
