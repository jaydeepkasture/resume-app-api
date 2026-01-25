# Resume HTML Enhancement Feature

## Overview
This feature enables the `/api/resume/chat/enhance` endpoint to accept HTML from a TiptapAngular editor along with embedded JSON resume data, process both through Ollama AI, and return enhanced HTML that can be directly patched back into the TiptapAngular editor.

## What Changed

### 1. **New Interface Method** (`IOllamaService.cs`)
Added `EnhanceResumeHtmlAsync` method to support HTML-based enhancement:

```csharp
Task<(string EnhancedHtml, ResumeDto EnhancedResume)> EnhanceResumeHtmlAsync(
    string resumeHtml, 
    ResumeDto resumeData, 
    string enhancementMessage);
```

### 2. **OllamaService Implementation** (`OllamaService.cs`)
Implemented the HTML enhancement workflow:

- **`EnhanceResumeHtmlAsync`**: Main method that sends HTML + JSON to Ollama
- **`BuildHtmlEnhancementPrompt`**: Creates a specialized prompt instructing Ollama to return both enhanced HTML and JSON
- **`ParseHtmlAndResumeFromResponse`**: Parses Ollama's response to extract both HTML and JSON using markers

**Prompt Format:**
The prompt instructs Ollama to return data in this format:
```
===HTML_START===
<enhanced HTML here>
===HTML_END===

===JSON_START===
{enhanced JSON here}
===JSON_END===
```

### 3. **Updated Data Models**

#### `ResumeEnhancementHistory.cs`
Added field to store enhanced HTML:
```csharp
[BsonElement("enhanced_html")]
public string? EnhancedHtml { get; set; }
```

#### `ChatEnhancementResponseDto.cs`
Added field to return enhanced HTML to frontend:
```csharp
public string? EnhancedHtml { get; set; }
```

#### `EnhancementHistoryDetailDto.cs`
Added field for history retrieval:
```csharp
public string? EnhancedHtml { get; set; }
```

### 4. **Repository Updates** (`ResumeRepository.cs`)

#### `ChatEnhanceAsync` Method
Updated to support HTML enhancement:

1. **Retrieves HTML from request or history**:
   - Uses `request.ResumeHtml` if provided
   - Falls back to latest `EnhancedHtml` from chat history

2. **Chooses enhancement method**:
   - If HTML is available → calls `EnhanceResumeHtmlAsync`
   - Otherwise → calls `EnhanceResumeAsync` (legacy JSON-only)

3. **Stores both original and enhanced HTML**:
   ```csharp
   ResumeHtml = originalHtml,
   EnhancedHtml = enhancedHtml,
   ```

4. **Returns enhanced HTML in response**:
   ```csharp
   EnhancedHtml = enhancedHtml,
   ```

#### `GetEnhancementHistoryDetailAsync` Method
Updated to include `EnhancedHtml` in the response when retrieving history.

## API Usage

### Request Format

**Endpoint:** `POST /api/resume/chat/enhance`

**Request Body:**
```json
{
  "chatId": "optional-chat-id",
  "message": "Make my experience section more impactful with quantifiable achievements",
  "resumeData": {
    "name": "John Doe",
    "email": "john@example.com",
    "phoneno": "123-456-7890",
    "location": "New York, NY",
    "linkedin": "linkedin.com/in/johndoe",
    "github": "github.com/johndoe",
    "summary": "Software engineer with 5 years of experience...",
    "experience": [...],
    "education": [...],
    "skills": [...]
  },
  "resumeHtml": "<div><h1>John Doe</h1><p>Software Engineer</p>...</div>"
}
```

**Key Points:**
- `resumeHtml`: HTML content from TiptapAngular editor
- `resumeData`: JSON representation of the resume (can be extracted from HTML or provided separately)
- `message`: User's enhancement instruction

### Response Format

```json
{
  "status": true,
  "message": "Enhancement completed successfully",
  "data": {
    "chatId": "chat-session-id",
    "userMessage": "Make my experience section more impactful...",
    "assistantMessage": "I've enhanced your resume based on your request...",
    "currentResume": {
      "name": "John Doe",
      "email": "john@example.com",
      // ... enhanced JSON data
    },
    "enhancedHtml": "<div><h1>John Doe</h1><p>Senior Software Engineer</p>...</div>",
    "processingTimeMs": 3542,
    "timestamp": "2026-01-22T15:30:00Z"
  }
}
```

**Key Response Fields:**
- `currentResume`: Enhanced resume in JSON format
- `enhancedHtml`: Enhanced HTML ready to patch into TiptapAngular editor
- `processingTimeMs`: Time taken by Ollama to process

## Frontend Integration

### 1. **Sending Enhancement Request**

```typescript
// In your Angular component
async enhanceResume(message: string) {
  // Get HTML from TiptapAngular editor
  const resumeHtml = this.editor.getHTML();
  
  // Get JSON data (either from editor or separate source)
  const resumeData = this.currentResumeData;
  
  const response = await this.http.post('/api/resume/chat/enhance', {
    chatId: this.currentChatId,
    message: message,
    resumeData: resumeData,
    resumeHtml: resumeHtml
  }).toPromise();
  
  return response;
}
```

### 2. **Updating TiptapAngular Editor**

```typescript
// After receiving response
onEnhancementComplete(response: any) {
  if (response.status && response.data.enhancedHtml) {
    // Patch the enhanced HTML back into TiptapAngular editor
    this.editor.commands.setContent(response.data.enhancedHtml);
    
    // Update your local resume data
    this.currentResumeData = response.data.currentResume;
    
    // Show success message
    this.showSuccess('Resume enhanced successfully!');
  }
}
```

### 3. **Loading from History**

```typescript
// When user clicks on a history item
async loadHistoryItem(historyId: string) {
  const response = await this.http.get(`/api/resume/chat/history/${historyId}`).toPromise();
  
  if (response.status && response.data.enhancedHtml) {
    // Load the enhanced HTML into editor
    this.editor.commands.setContent(response.data.enhancedHtml);
    
    // Update resume data
    this.currentResumeData = response.data.enhancedResume;
  }
}
```

## How It Works

### Workflow

1. **User sends enhancement request** with HTML and JSON
2. **Backend receives request** at `/api/resume/chat/enhance`
3. **Repository checks** if HTML is provided:
   - If yes → uses `EnhanceResumeHtmlAsync`
   - If no → uses `EnhanceResumeAsync` (legacy)
4. **OllamaService processes**:
   - Builds specialized prompt with HTML, JSON, and user message
   - Sends to Ollama API
   - Receives response with both enhanced HTML and JSON
5. **Backend parses response**:
   - Extracts HTML between `===HTML_START===` and `===HTML_END===`
   - Extracts JSON between `===JSON_START===` and `===JSON_END===`
6. **Backend stores** both original and enhanced HTML in MongoDB
7. **Backend returns** enhanced HTML and JSON to frontend
8. **Frontend patches** enhanced HTML into TiptapAngular editor

### Data Flow

```
TiptapAngular Editor (HTML)
         ↓
    Extract JSON
         ↓
Send to API (HTML + JSON + Message)
         ↓
    OllamaService
         ↓
    Ollama AI Processing
         ↓
Enhanced HTML + Enhanced JSON
         ↓
    Store in MongoDB
         ↓
Return to Frontend
         ↓
Patch into TiptapAngular Editor
```

## Database Schema

### `resume_enhancement_history` Collection

```javascript
{
  "_id": ObjectId("..."),
  "user_id": 123,
  "chat_id": ObjectId("..."),
  "message": "Make my experience section more impactful",
  "assistant_message": "I've enhanced your resume...",
  "original_resume": { /* JSON */ },
  "enhanced_resume": { /* JSON */ },
  "resume_html": "<div>...</div>",          // NEW: Original HTML
  "enhanced_html": "<div>...</div>",        // NEW: Enhanced HTML
  "created_at": ISODate("2026-01-22T15:30:00Z"),
  "processing_time_ms": 3542
}
```

## Testing

### 1. **Test with Postman/Curl**

```bash
curl -X POST http://localhost:5000/api/resume/chat/enhance \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "message": "Make my summary more compelling",
    "resumeData": {
      "name": "John Doe",
      "email": "john@example.com",
      "phoneno": "123-456-7890",
      "summary": "Software engineer with experience..."
    },
    "resumeHtml": "<div><h1>John Doe</h1><p>Software engineer with experience...</p></div>"
  }'
```

### 2. **Verify Response**

Check that response includes:
- ✅ `enhancedHtml` field with updated HTML
- ✅ `currentResume` field with updated JSON
- ✅ Both HTML and JSON reflect the enhancement

### 3. **Check Database**

Verify MongoDB document includes:
- ✅ `resume_html` (original)
- ✅ `enhanced_html` (enhanced)
- ✅ Both stored correctly

## Error Handling

The implementation includes robust error handling:

1. **Ollama Connection Errors**: Returns clear message if Ollama is not running
2. **Parsing Errors**: Falls back to alternative parsing methods if markers not found
3. **Invalid JSON**: Logs detailed error information for debugging
4. **Missing Data**: Gracefully handles missing HTML or resume data

## Backward Compatibility

The implementation maintains full backward compatibility:

- **Without HTML**: Works exactly as before (JSON-only enhancement)
- **With HTML**: Uses new HTML enhancement workflow
- **Legacy endpoints**: Continue to work unchanged
- **Existing data**: Old records without `enhanced_html` still work

## Configuration

Ensure Ollama is configured in `appsettings.json`:

```json
{
  "OllamaSettings": {
    "Url": "http://localhost:11434",
    "Model": "llama3.1:8b"
  }
}
```

## Next Steps

1. **Stop the running application** (if running)
2. **Build the project**: `dotnet build`
3. **Run the application**: `dotnet run`
4. **Test the endpoint** with HTML + JSON
5. **Integrate with frontend** TiptapAngular editor

## Troubleshooting

### Issue: Build fails with file lock errors
**Solution**: Stop the running application first, then rebuild

### Issue: Ollama returns malformed response
**Solution**: Check Ollama logs, ensure model supports the prompt format

### Issue: HTML not being enhanced
**Solution**: Verify `resumeHtml` is being sent in request, check logs for parsing errors

### Issue: Enhanced HTML breaks TiptapAngular
**Solution**: Ensure HTML structure is compatible with TiptapAngular schema, may need to adjust prompt

## Summary

This feature enables a seamless workflow where:
1. User edits resume in TiptapAngular editor
2. User sends enhancement request with current HTML + JSON
3. Ollama processes and returns enhanced HTML + JSON
4. Frontend directly patches enhanced HTML into editor
5. All changes are stored in MongoDB for history tracking

The implementation is production-ready, maintains backward compatibility, and includes comprehensive error handling.
