# Quick Start: Migration Execution

## ⚡ Fast Track Migration

### Prerequisites
- ✅ Application is running
- ✅ You have a valid JWT token
- ✅ You are logged in as an authenticated user

### 3-Step Migration Process

#### Step 1: Check Status (30 seconds)
```bash
# Using curl
curl -X GET "https://localhost:7200/api/admin/migration/chat-sessions/status" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"

# Or open in browser (after login)
https://localhost:7200/swagger
```

**Look for:**
```json
{
  "data": {
    "sessionsNeedingMigration": 5  // <-- If this is > 0, proceed to Step 2
  }
}
```

#### Step 2: Dry Run (1 minute)
```bash
curl -X GET "https://localhost:7200/api/admin/migration/chat-sessions/dry-run" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Verify:**
- No errors in response
- `migratedSessions` and `migratedMessages` counts look correct

#### Step 3: Execute Migration (2-5 minutes)
```bash
curl -X POST "https://localhost:7200/api/admin/migration/chat-sessions/execute" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Success Response:**
```json
{
  "status": true,
  "message": "Migration completed successfully! Migrated X sessions with Y messages"
}
```

---

## 🎯 Using Swagger UI (Easiest Method)

### 1. Start Application
```bash
cd D:\JD\C#\ResumeInOneMinute
dotnet run
```

### 2. Open Swagger
```
https://localhost:7200
```

### 3. Login First
- Find `/api/account/login` endpoint
- Click "Try it out"
- Enter your credentials:
  ```json
  {
    "email": "your@email.com",
    "password": "yourpassword"
  }
  ```
- Click "Execute"
- Copy the `token` from response

### 4. Authorize
- Click the "Authorize" button (🔒 icon at top)
- Enter: `Bearer YOUR_COPIED_TOKEN`
- Click "Authorize"
- Click "Close"

### 5. Run Migration
Navigate to **Admin** section and execute in order:

**A. Check Status**
- Expand `GET /api/admin/migration/chat-sessions/status`
- Click "Try it out"
- Click "Execute"
- Review response

**B. Dry Run**
- Expand `GET /api/admin/migration/chat-sessions/dry-run`
- Click "Try it out"
- Click "Execute"
- Verify no errors

**C. Execute Migration**
- Expand `POST /api/admin/migration/chat-sessions/execute`
- Click "Try it out"
- Click "Execute"
- Wait for completion
- Verify success message

**D. Verify**
- Run status check again
- `sessionsNeedingMigration` should be 0

---

## 📊 Expected Timeline

| Sessions | Messages | Estimated Time |
|----------|----------|----------------|
| 1-10     | <100     | 10-30 seconds  |
| 10-50    | <500     | 30-60 seconds  |
| 50-100   | <1000    | 1-2 minutes    |
| 100+     | 1000+    | 2-5 minutes    |

---

## ✅ Verification Checklist

After migration completes:

- [ ] Status shows `sessionsNeedingMigration: 0`
- [ ] Status shows `migrationComplete: true`
- [ ] Test: `GET /api/resume/chat/sessions` - lists your chats
- [ ] Test: `GET /api/resume/chat/{chatId}` - shows messages
- [ ] Test: `POST /api/resume/chat/enhance` - send new message works
- [ ] Check logs for any errors

---

## 🚨 If Something Goes Wrong

### Migration Fails
```json
{
  "status": false,
  "message": "Migration failed: ..."
}
```

**Actions:**
1. Check application logs in `Logs/` directory
2. Check MongoDB is running
3. Verify connection string in `appsettings.json`
4. Re-run migration (it's safe to retry)

### Partial Failure
```json
{
  "failedSessions": [
    { "chatId": "...", "error": "..." }
  ]
}
```

**Actions:**
1. Note the failed chat IDs
2. Check logs for detailed errors
3. Other sessions are migrated successfully
4. Can re-run migration to retry failed sessions

### No Sessions to Migrate
```json
{
  "sessionsNeedingMigration": 0
}
```

**This is normal if:**
- You have no chat sessions yet
- Migration already completed
- All sessions were created after the refactoring

---

## 🔄 Re-running Migration

**Safe to run multiple times:**
- ✅ Only processes sessions with embedded messages
- ✅ Skips already migrated sessions
- ✅ No duplicate data created
- ✅ Idempotent operation

---

## 📝 Quick MongoDB Verification

```javascript
// Connect to MongoDB
mongosh "mongodb://localhost:27017/ResumeInOneMinute"

// Check for sessions needing migration
db.chat_sessions.countDocuments({ messages: { $exists: true } })
// Should be 0 after migration

// Check migrated messages
db.resume_enhancement_history.countDocuments({ chat_id: { $exists: true } })
// Should show count of migrated messages

// View a sample migrated message
db.resume_enhancement_history.findOne({ chat_id: { $exists: true } })
```

---

## 🎉 Done!

Once migration is complete:
- ✅ Old embedded messages are now in `resume_enhancement_history`
- ✅ Chat sessions only contain metadata
- ✅ All APIs work as before
- ✅ System is ready for production use

**Next Steps:**
- Test your chat functionality
- Monitor application logs
- Enjoy unlimited messages per chat! 🚀
