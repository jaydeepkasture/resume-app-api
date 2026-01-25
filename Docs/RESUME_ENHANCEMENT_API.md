# Resume Enhancement API Documentation

## Overview
This API allows users to enhance their resumes using AI (Ollama LLM). It receives resume data in JSON format along with enhancement instructions, processes it through Ollama, and returns an enhanced version while maintaining a complete history in MongoDB.

## Features
- ✅ AI-powered resume enhancement using Ollama (local LLM)
- ✅ Complete history tracking in MongoDB
- ✅ JWT-based authentication
- ✅ Pagination support for history retrieval
- ✅ Processing time metrics
- ✅ Structured JSON input/output

## Prerequisites

### 1. Install and Run Ollama
```bash
# Download from https://ollama.com
# After installation, pull the model:
ollama pull llama3.1:8b

# Ollama will run automatically on http://localhost:11434
```

### 2. MongoDB Setup
Make sure MongoDB is running on `localhost:27017`. The database `resume_documents_dev` will be created automatically.

### 3. Configuration
The following settings are in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MongoDB": "mongodb://localhost:27017/resume_documents_dev"
  },
  "OllamaSettings": {
    "Url": "http://localhost:11434",
    "Model": "llama3.1:8b"
  }
}
```

## API Endpoints

### 1. Enhance Resume
**POST** `/api/resume/enhance`

Enhances a resume using AI based on provided instructions.

**Authentication:** Required (Bearer Token)

**Request Body:**
```json
{
  "resumeData": {
    "name": "Vicky Singh",
    "phoneno": "+91-8055193126",
    "email": "vicky.Singh@example.com",
    "location": "Mumbai, India",
    "linkedin": "linkedin.com/in/vickySingh",
    "github": "github.com/vickySingh",
    "summary": "Full Stack Developer with 3+ years of experience...",
    "experience": [
      {
        "company": "Google",
        "position": "Software Developer",
        "from": "2023-10",
        "to": "Present",
        "description": "Worked as a full stack developer..."
      }
    ],
    "skills": ["C#", "ASP.NET Core", "Angular"],
    "education": [
      {
        "degree": "Bachelor of Engineering",
        "field": "Computer Science",
        "institution": "XYZ Engineering College",
        "year": "2022"
      }
    ]
  },
  "enhancementInstruction": "Make the experience section more impactful with quantifiable achievements and action verbs. Focus on highlighting leadership and technical skills."
}
```

**Response (200 OK):**
```json
{
  "status": true,
  "message": "Resume enhanced successfully in 3542ms",
  "data": {
    "originalResume": { ... },
    "enhancedResume": {
      "name": "Vicky Singh",
      "phoneno": "+91-8055193126",
      "email": "vicky.Singh@example.com",
      "location": "Mumbai, India",
      "linkedin": "linkedin.com/in/vickySingh",
      "github": "github.com/vickySingh",
      "summary": "Results-driven Full Stack Developer with 3+ years...",
      "experience": [
        {
          "company": "Google",
          "position": "Software Developer",
          "from": "2023-10",
          "to": "Present",
          "description": "Led development of enterprise web applications using ASP.NET Core and Angular, resulting in 40% improvement in application performance. Architected and implemented RESTful APIs serving 10,000+ daily active users. Collaborated with cross-functional teams of 8+ developers to deliver features 2 weeks ahead of schedule."
        }
      ],
      "skills": ["C#", "ASP.NET Core", "Angular", "Leadership", "API Design"],
      "education": [ ... ]
    },
    "enhancementInstruction": "Make the experience section more impactful...",
    "historyId": "65f8a1b2c3d4e5f6a7b8c9d0",
    "processedAt": "2026-01-19T15:08:31.123Z"
  }
}
```

### 2. Get Enhancement History
**GET** `/api/resume/history?page=1&pageSize=10`

Retrieves user's resume enhancement history with pagination.

**Authentication:** Required (Bearer Token)

**Query Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 10, max: 50)

**Response (200 OK):**
```json
{
  "status": true,
  "message": "Retrieved 5 history records",
  "data": [
    {
      "originalResume": { ... },
      "enhancedResume": { ... },
      "enhancementInstruction": "...",
      "historyId": "65f8a1b2c3d4e5f6a7b8c9d0",
      "processedAt": "2026-01-19T15:08:31.123Z"
    }
  ]
}
```

### 3. Get Specific History Record
**GET** `/api/resume/history/{historyId}`

Retrieves a specific enhancement history record by ID.

**Authentication:** Required (Bearer Token)

**Path Parameters:**
- `historyId`: MongoDB ObjectId of the history record

**Response (200 OK):**
```json
{
  "status": true,
  "message": "History record retrieved successfully",
  "data": {
    "originalResume": { ... },
    "enhancedResume": { ... },
    "enhancementInstruction": "...",
    "historyId": "65f8a1b2c3d4e5f6a7b8c9d0",
    "processedAt": "2026-01-19T15:08:31.123Z"
  }
}
```

## Error Responses

### 400 Bad Request
```json
{
  "status": false,
  "message": "Validation failed",
  "data": ["Enhancement instruction is required", "Email is invalid"]
}
```

### 401 Unauthorized
```json
{
  "status": false,
  "message": "User not authenticated"
}
```

### 404 Not Found
```json
{
  "status": false,
  "message": "History record not found"
}
```

### 500 Internal Server Error (Ollama Connection)
```json
{
  "status": false,
  "message": "Failed to connect to Ollama at http://localhost:11434. Please ensure Ollama is running."
}
```

## MongoDB Schema

The enhancement history is stored in the `resume_enhancement_history` collection:

```javascript
{
  "_id": ObjectId("65f8a1b2c3d4e5f6a7b8c9d0"),
  "user_id": 123,
  "original_resume": { ... },
  "enhanced_resume": { ... },
  "enhancement_instruction": "Make the experience section more impactful...",
  "created_at": ISODate("2026-01-19T15:08:31.123Z"),
  "processing_time_ms": 3542
}
```

**Indexes:**
- `user_id` (ascending)
- `user_id` (ascending) + `created_at` (descending) - for efficient pagination

## Usage Example

### Step 1: Register/Login
```bash
POST /api/account/register
{
  "email": "user@example.com",
  "password": "YourPassword123!"
}
```

### Step 2: Get JWT Token
Copy the `token` from the response.

### Step 3: Enhance Resume
```bash
POST /api/resume/enhance
Headers:
  Authorization: Bearer {your-token}
Body:
{
  "resumeData": { ... },
  "enhancementInstruction": "Improve the summary section to highlight leadership skills"
}
```

### Step 4: View History
```bash
GET /api/resume/history?page=1&pageSize=10
Headers:
  Authorization: Bearer {your-token}
```

## Enhancement Instruction Examples

1. **General Enhancement:**
   - "Make the entire resume more professional and impactful"
   - "Optimize this resume for ATS (Applicant Tracking Systems)"

2. **Section-Specific:**
   - "Enhance the summary to highlight 5+ years of cloud architecture experience"
   - "Rewrite experience descriptions with quantifiable achievements"
   - "Add more technical skills relevant to DevOps roles"

3. **Job-Specific:**
   - "Tailor this resume for a Senior Backend Developer position at a fintech company"
   - "Emphasize machine learning and AI experience for a Data Scientist role"

4. **Formatting:**
   - "Make bullet points more concise, maximum 2 lines each"
   - "Use stronger action verbs in the experience section"

## Performance Considerations

- **Processing Time:** Typically 3-10 seconds depending on resume complexity and Ollama model
- **Rate Limiting:** Subject to global rate limits (100 requests per 60 seconds per user)
- **Timeout:** 5-minute timeout for Ollama API calls
- **History Pagination:** Maximum 50 records per page

## Troubleshooting

### Ollama Not Running
**Error:** "Failed to connect to Ollama"

**Solution:**
```bash
# Check if Ollama is running
curl http://localhost:11434/api/version

# If not, start Ollama (it usually starts automatically)
# On Windows: Ollama runs as a service
# On Mac/Linux: ollama serve
```

### Model Not Found
**Error:** "Model llama3.1:8b not found"

**Solution:**
```bash
ollama pull llama3.1:8b
```

### MongoDB Connection Issues
**Error:** "Failed to connect to MongoDB"

**Solution:**
- Ensure MongoDB is running on `localhost:27017`
- Check connection string in `appsettings.json`

## Comparison with Python Script

This C# implementation provides the same functionality as the provided Python script:

| Feature | Python Script | C# API |
|---------|--------------|--------|
| Ollama Integration | ✅ | ✅ |
| Resume Enhancement | ✅ | ✅ |
| JSON Input/Output | ✅ | ✅ |
| History Storage | ❌ | ✅ MongoDB |
| Authentication | ❌ | ✅ JWT |
| REST API | ❌ | ✅ |
| Multi-user Support | ❌ | ✅ |
| Pagination | ❌ | ✅ |
| Error Handling | Basic | Comprehensive |

## Architecture

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │ HTTP POST /api/resume/enhance
       ▼
┌─────────────────────┐
│ ResumeController    │ (JWT Auth)
└──────┬──────────────┘
       │
       ▼
┌─────────────────────┐
│ ResumeRepository    │
└──────┬──────────────┘
       │
       ├──────────────────┐
       │                  │
       ▼                  ▼
┌──────────────┐   ┌──────────────┐
│ OllamaService│   │   MongoDB    │
└──────┬───────┘   └──────────────┘
       │
       ▼
┌──────────────┐
│ Ollama LLM   │ (localhost:11434)
└──────────────┘
```

## Next Steps

1. **Test the API** using Swagger UI at `http://localhost:5000`
2. **Ensure Ollama is running** with the model pulled
3. **Create a user account** via `/api/account/register`
4. **Enhance your first resume** via `/api/resume/enhance`
5. **View your history** via `/api/resume/history`

## Support

For issues or questions:
- Check Swagger documentation at `/swagger`
- Review logs in the `Logs/` directory
- Ensure all prerequisites are met
