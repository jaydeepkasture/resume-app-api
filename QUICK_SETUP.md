# Quick Setup Guide - Groq API

## Step 1: Get Your Groq API Key

1. Go to: https://console.groq.com
2. Sign up / Log in
3. Navigate to **API Keys**
4. Click **Create API Key**
5. Copy the key (starts with `gsk_...`)

## Step 2: Set Environment Variable

### Windows PowerShell (Recommended)

**Temporary (Current Session Only):**
```powershell
$env:GROQ_API_KEY = "gsk_your_api_key_here"
```

**Permanent (User Level):**
```powershell
[System.Environment]::SetEnvironmentVariable('GROQ_API_KEY', 'gsk_your_api_key_here', 'User')
```

**Permanent (System Level - Requires Admin):**
```powershell
[System.Environment]::SetEnvironmentVariable('GROQ_API_KEY', 'gsk_your_api_key_here', 'Machine')
```

### Windows Command Prompt

**Temporary:**
```cmd
set GROQ_API_KEY=gsk_your_api_key_here
```

**Permanent:**
```cmd
setx GROQ_API_KEY "gsk_your_api_key_here"
```

### Linux / macOS

**Temporary:**
```bash
export GROQ_API_KEY=gsk_your_api_key_here
```

**Permanent (Add to ~/.bashrc or ~/.zshrc):**
```bash
echo 'export GROQ_API_KEY=gsk_your_api_key_here' >> ~/.bashrc
source ~/.bashrc
```

## Step 3: Verify Setup

**PowerShell:**
```powershell
echo $env:GROQ_API_KEY
```

**Command Prompt:**
```cmd
echo %GROQ_API_KEY%
```

**Linux/Mac:**
```bash
echo $GROQ_API_KEY
```

## Step 4: Restart Your Application

After setting the environment variable:

1. **Stop** the running application (Ctrl+C)
2. **Restart** the application
3. Check logs for: `✅ Groq API configured. Will use Groq as primary AI service with Ollama as fallback.`

## Alternative: Use appsettings.Development.json

Create or edit `appsettings.Development.json`:

```json
{
  "GroqSettings": {
    "ApiKey": "gsk_your_api_key_here",
    "BaseUrl": "https://api.groq.com/openai/v1",
    "Model": "llama-3.3-70b-versatile"
  }
}
```

⚠️ **Important**: Add `appsettings.Development.json` to `.gitignore` to avoid committing secrets!

## Troubleshooting

### "Groq API key not found" in logs

- Environment variable not set correctly
- Application not restarted after setting variable
- Using wrong shell/terminal

### "Invalid API key" error

- API key is incorrect
- API key has been revoked
- Check Groq console for key status

### Still using Ollama instead of Groq

- Check if environment variable is set: `echo $env:GROQ_API_KEY`
- Restart the application
- Check application logs for initialization message

## Quick Test

After setup, make an API call and check logs:

**Expected log output:**
```
✅ Groq API configured. Will use Groq as primary AI service with Ollama as fallback.
🚀 Attempting to enhance resume using Groq API (Model: llama-3.3-70b-versatile)
✅ Successfully enhanced resume using Groq API
```

---

**Need Help?** Check `GROQ_INTEGRATION.md` for detailed documentation.
