# Groq API Integration Guide

## Overview

The application now uses a **Composite AI Service** that intelligently tries **Groq API first** and automatically falls back to **local Ollama** if Groq is unavailable or fails.

## 🚀 Features

- ✅ **Primary**: Groq Cloud API (Fast, cloud-based LLM)
- ✅ **Fallback**: Local Ollama (Offline, privacy-focused)
- ✅ **Automatic Failover**: Seamless transition between services
- ✅ **Comprehensive Logging**: Clear visibility of which service is being used

## 📋 Configuration

### Option 1: Environment Variable (Recommended for Production)

Set the `GROQ_API_KEY` environment variable:

**Windows (PowerShell):**
```powershell
$env:GROQ_API_KEY = "your-groq-api-key-here"
```

**Windows (Command Prompt):**
```cmd
set GROQ_API_KEY=your-groq-api-key-here
```

**Linux/Mac:**
```bash
export GROQ_API_KEY=your-groq-api-key-here
```

### Option 2: appsettings.json (For Development)

Update `appsettings.json` or `appsettings.Development.json`:

```json
{
  "GroqSettings": {
    "ApiKey": "your-groq-api-key-here",
    "BaseUrl": "https://api.groq.com/openai/v1",
    "Model": "llama-3.3-70b-versatile"
  }
}
```

## 🔑 Getting a Groq API Key

1. Visit [https://console.groq.com](https://console.groq.com)
2. Sign up or log in
3. Navigate to **API Keys** section
4. Click **Create API Key**
5. Copy your API key

## 🎯 Available Groq Models

You can change the model in `appsettings.json`:

- `llama-3.3-70b-versatile` (Default - Best for general tasks)
- `llama-3.1-70b-versatile` (Alternative Llama model)
- `mixtral-8x7b-32768` (Mixtral model with large context)
- `gemma2-9b-it` (Google's Gemma model)

## 📊 How It Works

### Service Priority Flow

```
User Request
    ↓
Is Groq API Key configured?
    ↓
   YES → Try Groq API
    ↓
  Success? → Return Result ✅
    ↓
   NO → Fall back to Ollama
    ↓
  Return Result ✅
```

### Log Messages

The service uses emoji-rich logging for easy identification:

- 🚀 **Attempting Groq API** - Trying cloud service
- ✅ **Success with Groq** - Cloud service worked
- ⚠️ **Groq failed, falling back** - Switching to Ollama
- 🔄 **Using Ollama** - Using local service
- ❌ **Error** - Something went wrong

## 🔧 Testing

### Test with Groq (if configured)
```bash
# Make sure GROQ_API_KEY is set
curl -X POST http://localhost:5000/api/v1/resume/enhance \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{...}'
```

Check logs for: `🚀 Attempting to enhance resume using Groq API`

### Test Fallback to Ollama
```bash
# Temporarily remove or invalidate GROQ_API_KEY
# OR stop Groq service
# The system will automatically fall back to Ollama
```

Check logs for: `⚠️ Groq API failed. Falling back to local Ollama`

## 🛠️ Troubleshooting

### Groq API Not Being Used

**Check:**
1. Is `GROQ_API_KEY` set correctly?
   ```powershell
   echo $env:GROQ_API_KEY
   ```
2. Is the API key valid? (Check Groq console)
3. Check application logs for initialization message

### Both Services Failing

**Check:**
1. Groq API key validity
2. Ollama is running: `ollama serve`
3. Network connectivity
4. Check logs for specific error messages

### Performance Issues

**Groq is faster** than local Ollama for most models. If you're experiencing slow responses:
- Ensure Groq API key is configured
- Check network latency to Groq servers
- Consider switching to a smaller Groq model

## 📝 Code Changes Summary

### Files Modified:
1. ✅ `Program.cs` - Service registration updated
2. ✅ `appsettings.json` - Groq configuration added

### Files Created:
1. ✅ `CompositeAIService.cs` - Main composite service
2. ✅ `GroqService.cs` - Standalone Groq service (optional)

## 🔒 Security Best Practices

1. **Never commit API keys** to version control
2. Use **environment variables** in production
3. Use **appsettings.Development.json** for local development (add to `.gitignore`)
4. Rotate API keys regularly
5. Monitor API usage in Groq console

## 💡 Tips

- **Development**: Use Ollama (free, offline)
- **Production**: Use Groq (faster, cloud-based)
- **Hybrid**: Let the system auto-fallback for reliability

## 📞 Support

- Groq Documentation: https://console.groq.com/docs
- Ollama Documentation: https://ollama.ai/docs

---

**Last Updated**: 2026-02-05
