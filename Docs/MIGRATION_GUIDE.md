# Chat Session Migration Guide

## Overview
This guide will help you migrate existing chat sessions from the old embedded structure to the new separated structure.

## What Gets Migrated?

### Before Migration
**chat_sessions** collection:
```json
{
  "_id": "ObjectId(...)",
  "user_id": 123,
  "title": "My Resume Chat",
  "messages": [                    // ⚠️ Will be moved
    {
      "id": "guid",
      "role": "user",
      "content": "Enhance my resume",
      "resume_data": { ... },
      "timestamp": "..."
    },
    {
      "id": "guid",
      "role": "assistant",
      "content": "I've enhanced...",
      "resume_data": { ... },
      "timestamp": "..."
    }
  ],
  "current_resume": { ... },       // ⚠️ Will be removed
  "created_at": "...",
  "updated_at": "...",
  "is_active": true
}
```

### After Migration
**chat_sessions** collection:
```json
{
  "_id": "ObjectId(...)",
  "user_id": 123,
  "title": "My Resume Chat",
  "created_at": "...",
  "updated_at": "...",
  "is_active": true
}
```

**resume_enhancement_history** collection (new entries):
```json
{
  "_id": "ObjectId(...)",
  "user_id": 123,
  "chat_id": "ObjectId(...)",
  "role": "user",
  "message": "Enhance my resume",
  "resume_data": { ... },
  "created_at": "..."
}
{
  "_id": "ObjectId(...)",
  "user_id": 123,
  "chat_id": "ObjectId(...)",
  "role": "assistant",
  "message": "I've enhanced...",
  "resume_data": { ... },
  "created_at": "...",
  "processing_time_ms": 5000
}
```

## Migration Steps

### Step 1: Check Migration Status

**API Endpoint:**
```http
GET https://localhost:7200/api/admin/migration/chat-sessions/status
Authorization: Bearer {your-jwt-token}
```

**Expected Response:**
```json
{
  "status": true,
  "message": "Migration status retrieved",
  "data": {
    "totalChatSessions": 10,
    "sessionsNeedingMigration": 5,
    "sessionsAlreadyMigrated": 5,
    "migrationComplete": false
  }
}
```

### Step 2: Perform Dry Run (Recommended)

**API Endpoint:**
```http
GET https://localhost:7200/api/admin/migration/chat-sessions/dry-run
Authorization: Bearer {your-jwt-token}
```

**What it does:**
- ✅ Shows what would be migrated
- ✅ No changes to database
- ✅ Safe to run multiple times
- ✅ Provides preview of migration

**Expected Response:**
```json
{
  "status": true,
  "message": "Dry run completed. Would migrate 5 sessions with 23 messages",
  "data": {
    "totalSessionsFound": 5,
    "migratedSessions": 5,
    "migratedMessages": 23,
    "failedCount": 0
  }
}
```

### Step 3: Execute Migration

**⚠️ WARNING: This will modify your database!**

**API Endpoint:**
```http
POST https://localhost:7200/api/admin/migration/chat-sessions/execute
Authorization: Bearer {your-jwt-token}
```

**What it does:**
1. Finds all chat sessions with embedded `messages` array
2. For each message:
   - Creates a new entry in `resume_enhancement_history`
   - Preserves all message data (role, content, resume_data, timestamp, etc.)
3. Removes `messages` and `current_resume` fields from chat session
4. Keeps chat session metadata intact

**Expected Response:**
```json
{
  "status": true,
  "message": "Migration completed successfully! Migrated 5 sessions with 23 messages",
  "data": {
    "totalSessionsFound": 5,
    "migratedSessions": 5,
    "migratedMessages": 23,
    "failedSessions": [],
    "failedCount": 0
  }
}
```

### Step 4: Verify Migration

**Check status again:**
```http
GET https://localhost:7200/api/admin/migration/chat-sessions/status
```

**Expected Response (after successful migration):**
```json
{
  "status": true,
  "message": "Migration status retrieved",
  "data": {
    "totalChatSessions": 10,
    "sessionsNeedingMigration": 0,
    "sessionsAlreadyMigrated": 10,
    "migrationComplete": true
  }
}
```

## Using Swagger UI

1. **Start your application**
   ```bash
   dotnet run
   ```

2. **Open Swagger UI**
   ```
   https://localhost:7200
   ```

3. **Authenticate**
   - Click "Authorize" button
   - Enter: `Bearer {your-jwt-token}`
   - Click "Authorize"

4. **Navigate to Admin section**
   - Find "Admin" section in Swagger
   - You'll see three endpoints:
     - `GET /api/admin/migration/chat-sessions/status`
     - `GET /api/admin/migration/chat-sessions/dry-run`
     - `POST /api/admin/migration/chat-sessions/execute`

5. **Execute in order**
   - First: Check status
   - Second: Run dry-run
   - Third: Execute migration (if dry-run looks good)
   - Fourth: Verify with status check

## Using Postman

### 1. Check Status
```
GET https://localhost:7200/api/admin/migration/chat-sessions/status
Headers:
  Authorization: Bearer {your-jwt-token}
```

### 2. Dry Run
```
GET https://localhost:7200/api/admin/migration/chat-sessions/dry-run
Headers:
  Authorization: Bearer {your-jwt-token}
```

### 3. Execute Migration
```
POST https://localhost:7200/api/admin/migration/chat-sessions/execute
Headers:
  Authorization: Bearer {your-jwt-token}
```

## Troubleshooting

### Migration Fails for Some Sessions

If you see failed sessions in the response:
```json
{
  "failedSessions": [
    {
      "chatId": "ObjectId(...)",
      "error": "Error message here"
    }
  ]
}
```

**What to do:**
1. Check the application logs for detailed error messages
2. The migration continues for other sessions even if some fail
3. You can re-run the migration - it will only process sessions that still have embedded messages
4. Failed sessions remain in their original state

### No Sessions Found

If `sessionsNeedingMigration` is 0:
- ✅ Either you have no chat sessions yet
- ✅ Or migration is already complete
- ✅ This is not an error

### Permission Denied

If you get 401 Unauthorized:
- Make sure you're logged in
- Get a fresh JWT token from `/api/account/login`
- Include `Bearer ` prefix in Authorization header

## Backup Recommendation

**Before running migration in production:**

1. **Backup MongoDB database:**
   ```bash
   mongodump --uri="mongodb://localhost:27017/ResumeInOneMinute" --out=backup_before_migration
   ```

2. **Run migration on a copy first:**
   - Test on development/staging environment
   - Verify everything works correctly
   - Then run on production

3. **Restore if needed:**
   ```bash
   mongorestore --uri="mongodb://localhost:27017/ResumeInOneMinute" backup_before_migration/ResumeInOneMinute
   ```

## Post-Migration Verification

### Verify in MongoDB

```javascript
// Check chat sessions no longer have messages
db.chat_sessions.findOne({ messages: { $exists: true } })
// Should return null

// Check messages are in history
db.resume_enhancement_history.find({ chat_id: { $exists: true } }).count()
// Should show count of migrated messages

// Verify a specific chat
var chatId = ObjectId("your-chat-id");
db.chat_sessions.findOne({ _id: chatId })
db.resume_enhancement_history.find({ chat_id: chatId }).sort({ created_at: 1 })
```

### Test API Endpoints

1. **List chat sessions:**
   ```
   GET /api/resume/chat/sessions
   ```

2. **Get a specific chat:**
   ```
   GET /api/resume/chat/{chatId}
   ```
   - Should return all messages from history
   - Should show current resume from latest history entry

3. **Send a new message:**
   ```
   POST /api/resume/chat/enhance
   ```
   - Should work normally
   - New messages go to history collection

## Migration is Idempotent

✅ **Safe to run multiple times**
- Only processes sessions with embedded messages
- Already migrated sessions are skipped
- No duplicate data created

## Rollback (If Needed)

If you need to rollback:

1. **Restore from backup:**
   ```bash
   mongorestore --uri="mongodb://localhost:27017/ResumeInOneMinute" backup_before_migration/ResumeInOneMinute
   ```

2. **Or manually:**
   - Delete migrated history entries: `db.resume_enhancement_history.deleteMany({ chat_id: { $exists: true } })`
   - Restore from backup

## Support

If you encounter issues:
1. Check application logs in `Logs/` directory
2. Check MongoDB logs
3. Verify MongoDB connection
4. Ensure you have proper permissions

## Summary

✅ **Before Migration:** Messages embedded in chat_sessions  
✅ **After Migration:** Messages in separate resume_enhancement_history collection  
✅ **Benefits:** Scalability, no document size limits, better performance  
✅ **Safe:** Dry-run available, idempotent, preserves all data  
