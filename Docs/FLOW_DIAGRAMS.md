# HTML Enhancement Feature - Visual Flow Diagrams

## 1. Overall Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         FRONTEND                                 │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │         TiptapAngular Editor Component                   │   │
│  │  - User edits resume                                     │   │
│  │  - Extracts HTML: editor.getHTML()                       │   │
│  │  - Extracts JSON: currentResumeData                      │   │
│  └────────────────────┬─────────────────────────────────────┘   │
│                       │                                          │
│                       │ POST /api/resume/chat/enhance            │
│                       │ { message, resumeData, resumeHtml }      │
└───────────────────────┼──────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────────┐
│                         BACKEND                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │              ResumeController                            │   │
│  │  - Validates JWT token                                   │   │
│  │  - Extracts userId                                       │   │
│  │  - Calls repository                                      │   │
│  └────────────────────┬─────────────────────────────────────┘   │
│                       │                                          │
│                       ▼                                          │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │              ResumeRepository                            │   │
│  │  - Gets/creates chat session                             │   │
│  │  - Retrieves chat history                                │   │
│  │  - Determines enhancement method                         │   │
│  │  - Calls OllamaService                                   │   │
│  │  - Stores results in MongoDB                             │   │
│  └────────────────────┬─────────────────────────────────────┘   │
│                       │                                          │
│                       ▼                                          │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │              OllamaService                               │   │
│  │  - Builds specialized prompt                             │   │
│  │  - Calls Ollama API                                      │   │
│  │  - Parses response (HTML + JSON)                         │   │
│  └────────────────────┬─────────────────────────────────────┘   │
└───────────────────────┼──────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────────┐
│                      OLLAMA AI                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  - Receives HTML + JSON + Enhancement Message            │   │
│  │  - Processes with LLM (llama3.1:8b)                      │   │
│  │  - Returns Enhanced HTML + Enhanced JSON                 │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────────┐
│                      MONGODB                                     │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │  Collection: resume_enhancement_history                  │   │
│  │  - original_resume (JSON)                                │   │
│  │  - enhanced_resume (JSON)                                │   │
│  │  - resume_html (Original HTML)                           │   │
│  │  - enhanced_html (Enhanced HTML) ← NEW                   │   │
│  └──────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
```

## 2. Request Processing Flow

```
START
  │
  ▼
┌─────────────────────────┐
│ Receive API Request     │
│ - chatId (optional)     │
│ - message (required)    │
│ - resumeData (optional) │
│ - resumeHtml (optional) │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Validate JWT Token      │
│ Extract userId          │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Get/Create Chat Session │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Retrieve Chat History   │
│ from MongoDB            │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Determine Resume Data   │
│ - From request?         │
│ - From history?         │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Determine HTML          │
│ - From request?         │
│ - From history?         │
└──────────┬──────────────┘
           │
           ▼
      ┌────┴────┐
      │ HTML?   │
      └────┬────┘
           │
     ┌─────┴─────┐
     │           │
    YES         NO
     │           │
     ▼           ▼
┌─────────┐  ┌─────────┐
│ HTML    │  │ JSON    │
│ Enhance │  │ Enhance │
│ Method  │  │ Method  │
└────┬────┘  └────┬────┘
     │           │
     └─────┬─────┘
           │
           ▼
┌─────────────────────────┐
│ Call Ollama API         │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Parse Response          │
│ - Extract HTML          │
│ - Extract JSON          │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Store in MongoDB        │
│ - Original data         │
│ - Enhanced data         │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Update Chat Session     │
│ - UpdatedAt             │
│ - Title (if first msg)  │
└──────────┬──────────────┘
           │
           ▼
┌─────────────────────────┐
│ Return Response         │
│ - chatId                │
│ - enhancedHtml          │
│ - currentResume         │
│ - processingTimeMs      │
└──────────┬──────────────┘
           │
           ▼
          END
```

## 3. HTML Enhancement Method Detail

```
EnhanceResumeHtmlAsync(resumeHtml, resumeData, message)
  │
  ▼
┌─────────────────────────────────────────────┐
│ Build Specialized Prompt                    │
│                                              │
│ You are an expert resume writer...          │
│                                              │
│ CURRENT RESUME HTML:                         │
│ {resumeHtml}                                 │
│                                              │
│ CURRENT RESUME JSON:                         │
│ {resumeData}                                 │
│                                              │
│ USER'S ENHANCEMENT REQUEST:                  │
│ {message}                                    │
│                                              │
│ YOUR RESPONSE MUST CONTAIN:                  │
│ ===HTML_START===                             │
│ [enhanced HTML]                              │
│ ===HTML_END===                               │
│                                              │
│ ===JSON_START===                             │
│ [enhanced JSON]                              │
│ ===JSON_END===                               │
└──────────────────┬──────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────┐
│ Send to Ollama API                           │
│ POST http://localhost:11434/api/generate     │
│ {                                            │
│   "model": "llama3.1:8b",                    │
│   "prompt": "...",                           │
│   "stream": false,                           │
│   "options": {                               │
│     "temperature": 0.3,                      │
│     "top_p": 0.9                             │
│   }                                          │
│ }                                            │
└──────────────────┬──────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────┐
│ Receive Ollama Response                      │
│ {                                            │
│   "model": "llama3.1:8b",                    │
│   "response": "===HTML_START===...",         │
│   "done": true                               │
│ }                                            │
└──────────────────┬──────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────┐
│ Parse Response                               │
│                                              │
│ 1. Find ===HTML_START=== marker              │
│ 2. Extract HTML until ===HTML_END===         │
│ 3. Find ===JSON_START=== marker              │
│ 4. Extract JSON until ===JSON_END===         │
│ 5. Clean JSON (remove markdown if present)   │
│ 6. Deserialize JSON to ResumeDto             │
│                                              │
│ Fallback if markers not found:               │
│ - Find first < to last > for HTML            │
│ - Find first { to last } for JSON            │
└──────────────────┬──────────────────────────┘
                   │
                   ▼
┌─────────────────────────────────────────────┐
│ Return Tuple                                 │
│ (enhancedHtml, enhancedResume)               │
└─────────────────────────────────────────────┘
```

## 4. Data Flow Diagram

```
┌──────────────┐
│   Frontend   │
│  TiptapEditor│
└──────┬───────┘
       │
       │ 1. Get HTML
       │    editor.getHTML()
       │
       ▼
┌──────────────┐
│  HTML String │
│ "<div>...</>" │
└──────┬───────┘
       │
       │ 2. Get JSON
       │    currentResumeData
       │
       ▼
┌──────────────┐
│  JSON Object │
│  { name: ... }│
└──────┬───────┘
       │
       │ 3. Send to API
       │    POST /api/resume/chat/enhance
       │
       ▼
┌──────────────────────────────────┐
│         Backend API              │
│  ┌────────────────────────────┐  │
│  │  ChatEnhanceAsync          │  │
│  │  - Validate                │  │
│  │  - Get history             │  │
│  │  - Determine method        │  │
│  └────────┬───────────────────┘  │
│           │                      │
│           ▼                      │
│  ┌────────────────────────────┐  │
│  │  EnhanceResumeHtmlAsync    │  │
│  │  - Build prompt            │  │
│  │  - Call Ollama             │  │
│  │  - Parse response          │  │
│  └────────┬───────────────────┘  │
└───────────┼──────────────────────┘
            │
            ▼
┌───────────────────────────────┐
│        Ollama AI              │
│  - Process HTML + JSON        │
│  - Apply enhancements         │
│  - Return enhanced versions   │
└───────────┬───────────────────┘
            │
            ▼
┌───────────────────────────────┐
│    Enhanced HTML + JSON       │
│  ===HTML_START===             │
│  <div class="enhanced">...    │
│  ===HTML_END===               │
│                               │
│  ===JSON_START===             │
│  { "name": "Enhanced..." }    │
│  ===JSON_END===               │
└───────────┬───────────────────┘
            │
            ▼
┌───────────────────────────────┐
│      Parse & Store            │
│  ┌─────────────────────────┐  │
│  │  MongoDB                │  │
│  │  - original_resume      │  │
│  │  - enhanced_resume      │  │
│  │  - resume_html          │  │
│  │  - enhanced_html ← NEW  │  │
│  └─────────────────────────┘  │
└───────────┬───────────────────┘
            │
            ▼
┌───────────────────────────────┐
│      Return to Frontend       │
│  {                            │
│    enhancedHtml: "...",       │
│    currentResume: {...}       │
│  }                            │
└───────────┬───────────────────┘
            │
            ▼
┌───────────────────────────────┐
│    Update TiptapEditor        │
│  editor.commands.setContent(  │
│    response.data.enhancedHtml │
│  )                            │
└───────────────────────────────┘
```

## 5. Decision Tree: Enhancement Method Selection

```
                    START
                      │
                      ▼
              ┌───────────────┐
              │ Request has   │
              │ resumeHtml?   │
              └───────┬───────┘
                      │
              ┌───────┴───────┐
              │               │
             YES             NO
              │               │
              ▼               ▼
      ┌──────────────┐  ┌──────────────┐
      │ Request has  │  │ Check history│
      │ resumeData?  │  │ for HTML     │
      └──────┬───────┘  └──────┬───────┘
             │                 │
      ┌──────┴──────┐   ┌──────┴──────┐
      │             │   │             │
     YES           NO   YES           NO
      │             │   │             │
      ▼             ▼   ▼             ▼
  ┌────────┐  ┌────────┐ ┌────────┐ ┌────────┐
  │ HTML   │  │ Get    │ │ HTML   │ │ JSON   │
  │ Enhance│  │ Resume │ │ Enhance│ │ Enhance│
  │ Method │  │ from   │ │ Method │ │ Method │
  │        │  │ History│ │        │ │        │
  └────┬───┘  └───┬────┘ └────┬───┘ └────┬───┘
       │          │           │          │
       │          ▼           │          │
       │      ┌────────┐      │          │
       │      │ HTML   │      │          │
       │      │ Enhance│      │          │
       │      │ Method │      │          │
       │      └───┬────┘      │          │
       │          │           │          │
       └──────────┴───────────┴──────────┘
                      │
                      ▼
              ┌──────────────┐
              │ Call Ollama  │
              │ Service      │
              └──────┬───────┘
                     │
                     ▼
                   RESULT
```

## 6. MongoDB Document Structure

```
┌─────────────────────────────────────────────────────────────┐
│  Collection: resume_enhancement_history                     │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  {                                                           │
│    "_id": ObjectId("..."),                                   │
│    "user_id": 123,                                           │
│    "chat_id": ObjectId("..."),                               │
│                                                              │
│    // User interaction                                       │
│    "message": "Make my experience more impactful",           │
│    "assistant_message": "I've enhanced your resume...",      │
│                                                              │
│    // JSON data                                              │
│    "original_resume": {                                      │
│      "name": "John Doe",                                     │
│      "email": "john@example.com",                            │
│      ...                                                     │
│    },                                                        │
│    "enhanced_resume": {                                      │
│      "name": "John Doe",                                     │
│      "email": "john@example.com",                            │
│      "summary": "Enhanced summary...",                       │
│      ...                                                     │
│    },                                                        │
│                                                              │
│    // HTML data (NEW)                                        │
│    "resume_html": "<div><h1>John Doe</h1>...</div>",         │
│    "enhanced_html": "<div><h1>John Doe</h1>                  │
│                      <p>Enhanced...</p>...</div>",           │
│                                                              │
│    // Metadata                                               │
│    "created_at": ISODate("2026-01-22T15:30:00Z"),            │
│    "processing_time_ms": 3542                                │
│  }                                                           │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## 7. Error Handling Flow

```
                    START
                      │
                      ▼
              ┌───────────────┐
              │ Try Block     │
              └───────┬───────┘
                      │
        ┌─────────────┼─────────────┐
        │             │             │
        ▼             ▼             ▼
  ┌──────────┐  ┌──────────┐  ┌──────────┐
  │ Validate │  │ Process  │  │ Store    │
  │ Input    │  │ with     │  │ in DB    │
  │          │  │ Ollama   │  │          │
  └────┬─────┘  └────┬─────┘  └────┬─────┘
       │             │             │
       │ Error?      │ Error?      │ Error?
       │             │             │
       ▼             ▼             ▼
  ┌──────────────────────────────────┐
  │        Catch Block               │
  │  - Log error details             │
  │  - Return error response         │
  │  - Include helpful message       │
  └──────────────┬───────────────────┘
                 │
                 ▼
         ┌───────────────┐
         │ Error Types:  │
         │               │
         │ 1. Auth Error │
         │    → 401      │
         │               │
         │ 2. Validation │
         │    → 400      │
         │               │
         │ 3. Ollama     │
         │    → 400      │
         │               │
         │ 4. Parse      │
         │    → 400      │
         │               │
         │ 5. Database   │
         │    → 500      │
         └───────────────┘
```

## Legend

```
┌─────────┐
│ Process │  = Processing step
└─────────┘

┌─────────┐
│Decision?│  = Decision point
└─────────┘

    │
    ▼        = Data flow direction

┌─────────────────────┐
│  Component/Service  │  = System component
└─────────────────────┘
```
