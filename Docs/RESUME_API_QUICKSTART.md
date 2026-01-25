# Resume Enhancement API - Quick Start Guide

## 🚀 What's New

A new **Resume Enhancement API** has been added that uses **Ollama (local LLM)** to enhance resumes with AI. This is similar to the Python script you provided, but implemented as a full-featured REST API in C#.

## ✨ Features

- 🤖 **AI-Powered Enhancement**: Uses Ollama's LLM (llama3.1:8b) to improve resumes
- 📝 **JSON Input/Output**: Same structure as your Python script
- 💾 **MongoDB History**: Automatically saves all enhancements
- 🔐 **Secure**: JWT authentication required
- 📊 **Metrics**: Tracks processing time for each enhancement
- 📄 **Pagination**: Browse through enhancement history

## 📋 Prerequisites

### 1. Install Ollama

**Windows:**
```powershell
# Download and install from https://ollama.com
# After installation, pull the model:
ollama pull llama3.1:8b
```

**Mac/Linux:**
```bash
# Install Ollama
curl -fsSL https://ollama.com/install.sh | sh

# Pull the model
ollama pull llama3.1:8b
```

Ollama will automatically run on `http://localhost:11434`

### 2. Verify Ollama is Running

```bash
curl http://localhost:11434/api/version
```

### 3. MongoDB

Ensure MongoDB is running on `localhost:27017`. The database will be created automatically.

## 🏃 Running the Application

```bash
cd ResumeInOneMinute
dotnet run
```

The API will be available at: `http://localhost:5000`

Swagger UI: `http://localhost:5000/swagger`

## 📚 API Endpoints

### 1. Enhance Resume
```http
POST /api/resume/enhance
Authorization: Bearer {token}
Content-Type: application/json

{
  "resumeData": {
    "name": "Vicky Singh",
    "phoneno": "+91-8055193126",
    "email": "vicky.Singh@example.com",
    "location": "Mumbai, India",
    "linkedin": "linkedin.com/in/vickySingh",
    "github": "github.com/vickySingh",
    "summary": "Full Stack Developer with 3+ years...",
    "experience": [...],
    "skills": [...],
    "education": [...]
  },
  "enhancementInstruction": "Make the experience section more impactful with quantifiable achievements"
}
```

### 2. Get History
```http
GET /api/resume/history?page=1&pageSize=10
Authorization: Bearer {token}
```

### 3. Get Specific History
```http
GET /api/resume/history/{historyId}
Authorization: Bearer {token}
```

## 🧪 Testing

### Option 1: Using Swagger UI
1. Navigate to `http://localhost:5000`
2. Click "Authorize" and enter your JWT token
3. Try the `/api/resume/enhance` endpoint

### Option 2: Using Python Test Script
```bash
# Make sure the API is running first
python test_resume_api.py
```

### Option 3: Using cURL
```bash
# 1. Register
curl -X POST http://localhost:5000/api/account/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test@123456"}'

# 2. Copy the token from response

# 3. Enhance Resume
curl -X POST http://localhost:5000/api/resume/enhance \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d @sample_resume_request.json
```

## 📁 Files Created

### Domain Layer
- `ResumeInOneMinute.Domain/DTO/ResumeDto.cs` - Resume data structure
- `ResumeInOneMinute.Domain/DTO/ResumeEnhancementRequestDto.cs` - Request DTO
- `ResumeInOneMinute.Domain/DTO/ResumeEnhancementResponseDto.cs` - Response DTO
- `ResumeInOneMinute.Domain/Model/ResumeEnhancementHistory.cs` - MongoDB model
- `ResumeInOneMinute.Domain/Interface/IResumeRepository.cs` - Repository interface

### Infrastructure Layer
- `ResumeInOneMinute.Infrastructure/Services/IOllamaService.cs` - Service interface
- `ResumeInOneMinute.Infrastructure/Services/OllamaService.cs` - Ollama integration

### Repository Layer
- `ResumeInOneMinute.Repository/Repositories/ResumeRepository.cs` - MongoDB operations

### API Layer
- `ResumeInOneMinute/Controllers/Resume/ResumeController.cs` - API endpoints

### Documentation
- `Docs/RESUME_ENHANCEMENT_API.md` - Comprehensive API documentation
- `test_resume_api.py` - Python test script

## 🔧 Configuration

All settings are in `appsettings.json`:

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

## 📊 MongoDB Collections

### resume_enhancement_history
```javascript
{
  "_id": ObjectId("..."),
  "user_id": 123,
  "original_resume": { ... },
  "enhanced_resume": { ... },
  "enhancement_instruction": "...",
  "created_at": ISODate("..."),
  "processing_time_ms": 3542
}
```

**Indexes:**
- `user_id` (ascending)
- `user_id` + `created_at` (compound, for pagination)

## 🎯 Example Enhancement Instructions

1. **General:**
   - "Make the entire resume more professional and impactful"
   - "Optimize for ATS (Applicant Tracking Systems)"

2. **Section-Specific:**
   - "Enhance the summary to highlight leadership skills"
   - "Add quantifiable achievements to experience section"
   - "Improve skills section with relevant technologies"

3. **Job-Specific:**
   - "Tailor for a Senior Backend Developer role"
   - "Emphasize cloud and DevOps experience"

## ⚡ Performance

- **Processing Time:** 3-15 seconds (depends on resume complexity)
- **Timeout:** 5 minutes for Ollama API calls
- **Rate Limiting:** 100 requests per 60 seconds per user

## 🐛 Troubleshooting

### Ollama Not Running
```bash
# Check if Ollama is running
curl http://localhost:11434/api/version

# If not running, start it (usually auto-starts)
# Windows: Check Windows Services
# Mac/Linux: ollama serve
```

### Model Not Found
```bash
ollama pull llama3.1:8b
```

### MongoDB Connection Error
- Ensure MongoDB is running on port 27017
- Check connection string in `appsettings.json`

### Build Errors
```bash
# Restore packages
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build
```

## 📖 Full Documentation

See `Docs/RESUME_ENHANCEMENT_API.md` for:
- Detailed API reference
- Request/response examples
- Error handling
- Architecture diagrams
- Advanced usage

## 🔄 Comparison with Python Script

| Feature | Python Script | C# API |
|---------|--------------|--------|
| Ollama Integration | ✅ | ✅ |
| Resume Enhancement | ✅ | ✅ |
| JSON I/O | ✅ | ✅ |
| History Storage | ❌ | ✅ MongoDB |
| Authentication | ❌ | ✅ JWT |
| REST API | ❌ | ✅ |
| Multi-user | ❌ | ✅ |
| Pagination | ❌ | ✅ |

## 🎉 Next Steps

1. ✅ Start the application: `dotnet run`
2. ✅ Ensure Ollama is running with model pulled
3. ✅ Register a user account
4. ✅ Test the enhancement endpoint
5. ✅ Check your enhancement history

## 💡 Tips

- **First Enhancement:** May take longer as Ollama loads the model
- **Instruction Quality:** More specific instructions = better results
- **Token Management:** JWT tokens expire after 60 minutes (configurable)
- **History:** All enhancements are saved automatically for future reference

## 📞 Support

- Check Swagger docs at `/swagger`
- Review logs in `Logs/` directory
- See full documentation in `Docs/RESUME_ENHANCEMENT_API.md`

---

**Happy Resume Enhancing! 🚀**
