# Testing Resources

This folder contains all test-related files and resources for the ResumeInOneMinute API.

## 📁 Contents

### 1. **test_resume_api.py**
Python script for testing the Resume Enhancement API endpoints.

**Usage:**
```bash
python Testing/test_resume_api.py
```

**What it tests:**
- User registration
- User login
- Resume enhancement with Ollama
- History retrieval

**Prerequisites:**
- Python 3.x
- `requests` library: `pip install requests`
- API running on `http://localhost:5000`
- Ollama running with `llama3.1:8b` model

### 2. **sample_resume_request.json**
Sample JSON payload for testing the resume enhancement endpoint.

**Usage:**
```bash
# Using cURL
curl -X POST http://localhost:5000/api/resume/enhance \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d @Testing/sample_resume_request.json

# Using PowerShell
Invoke-RestMethod -Uri "http://localhost:5000/api/resume/enhance" `
  -Method Post `
  -Headers @{"Authorization"="Bearer YOUR_TOKEN"} `
  -ContentType "application/json" `
  -InFile "Testing/sample_resume_request.json"
```

**Structure:**
```json
{
  "resumeData": {
    "name": "...",
    "email": "...",
    "experience": [...],
    "skills": [...],
    "education": [...]
  },
  "enhancementInstruction": "Make it more impactful"
}
```

### 3. **ResumeInOneMinute.postman_collection.json**
Postman collection with all API endpoints pre-configured.

**Usage:**
1. Open Postman
2. Import → Upload Files → Select `ResumeInOneMinute.postman_collection.json`
3. Set environment variables:
   - `baseUrl`: `http://localhost:5000`
   - `token`: Your JWT token (from login/register)

**Included Endpoints:**
- Account Management
  - Register
  - Login
  - Get Profile
  - Update Profile
  - Forgot Password
  - Reset Password
- Resume Enhancement
  - Enhance Resume
  - Get History
  - Get History by ID

## 🚀 Quick Start

### Option 1: Python Script (Automated)
```bash
# Install dependencies
pip install requests

# Run the test script
python Testing/test_resume_api.py
```

### Option 2: Postman (Manual)
1. Import the Postman collection
2. Register a new user
3. Copy the JWT token
4. Test other endpoints

### Option 3: cURL (Command Line)
```bash
# Register
curl -X POST http://localhost:5000/api/account/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test@123456"}'

# Enhance Resume
curl -X POST http://localhost:5000/api/resume/enhance \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d @Testing/sample_resume_request.json
```

## 📝 Creating Your Own Test Data

### Custom Resume Request
Copy `sample_resume_request.json` and modify:
```json
{
  "resumeData": {
    "name": "Your Name",
    "phoneno": "+1-234-567-8900",
    "email": "your.email@example.com",
    "location": "Your City, Country",
    "linkedin": "linkedin.com/in/yourprofile",
    "github": "github.com/yourprofile",
    "summary": "Your professional summary...",
    "experience": [
      {
        "company": "Company Name",
        "position": "Your Position",
        "from": "Jan 2020",
        "to": "Present",
        "description": "Your achievements and responsibilities..."
      }
    ],
    "skills": ["Skill1", "Skill2", "Skill3"],
    "education": [
      {
        "degree": "Your Degree",
        "field": "Your Field",
        "institution": "University Name",
        "year": "2020"
      }
    ]
  },
  "enhancementInstruction": "Your specific enhancement request"
}
```

## 🧪 Test Scenarios

### 1. Basic Flow Test
```python
# 1. Register user
# 2. Login
# 3. Enhance resume
# 4. Get history
# 5. Get specific history item
```

### 2. Error Handling Test
```python
# 1. Invalid credentials
# 2. Missing required fields
# 3. Invalid JWT token
# 4. Malformed JSON
```

### 3. Performance Test
```python
# 1. Multiple concurrent requests
# 2. Large resume data
# 3. Complex enhancement instructions
```

## 📊 Expected Response Times

- **Registration/Login:** < 500ms
- **Resume Enhancement:** 3-15 seconds (depends on Ollama)
- **History Retrieval:** < 200ms
- **Profile Operations:** < 300ms

## 🐛 Troubleshooting

### Issue: "Connection refused"
**Solution:** Ensure the API is running on `http://localhost:5000`
```bash
cd ResumeInOneMinute
dotnet run
```

### Issue: "Ollama error"
**Solution:** Ensure Ollama is running
```bash
# Check Ollama status
curl http://localhost:11434/api/version

# Pull model if needed
ollama pull llama3.1:8b
```

### Issue: "Unauthorized"
**Solution:** Get a fresh JWT token
```bash
# Register or login to get new token
curl -X POST http://localhost:5000/api/account/login \
  -H "Content-Type: application/json" \
  -d '{"email":"your@email.com","password":"YourPassword"}'
```

## 📚 Additional Resources

- **API Documentation:** See `Docs/` folder
- **Swagger UI:** `http://localhost:5000` (when API is running)
- **Postman Documentation:** `https://www.postman.com/`

## 💡 Tips

1. **Use Swagger UI** for interactive testing during development
2. **Use Postman** for organized test collections
3. **Use Python script** for automated testing and CI/CD
4. **Keep test data** separate from production data
5. **Update tokens** regularly as they expire after 60 minutes

---

**Note:** All test files should remain in this `Testing/` folder to keep the project root clean and organized.
