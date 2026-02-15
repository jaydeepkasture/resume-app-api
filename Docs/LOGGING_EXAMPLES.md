# Example Log Output

## Scenario 1: Groq API Configured and Working

```
[12:30:45 INF] ✅ Groq API configured. Will use Groq as primary AI service with Ollama as fallback.
[12:30:50 INF] 🚀 Attempting to enhance resume using Groq API (Model: llama-3.3-70b-versatile)
[12:30:51 DBG] 📤 Sending request to Groq API: https://api.groq.com/openai/v1/chat/completions
[12:30:52 DBG] 📥 Received response from Groq API. Length: 2456
[12:30:52 DBG] 🔍 Parsing AI response. Length: 2456
[12:30:52 INF] ✅ Successfully parsed resume. Name: John Doe
[12:30:52 INF] ✅ Successfully enhanced resume using Groq API
```

## Scenario 2: Groq API Fails, Falls Back to Ollama

```
[12:35:10 INF] ✅ Groq API configured. Will use Groq as primary AI service with Ollama as fallback.
[12:35:15 INF] 🚀 Attempting to enhance resume using Groq API (Model: llama-3.3-70b-versatile)
[12:35:16 DBG] 📤 Sending request to Groq API: https://api.groq.com/openai/v1/chat/completions
[12:35:17 ERR] ❌ Groq API error: 429 - Rate limit exceeded
[12:35:17 WRN] ⚠️ Groq API failed. Falling back to local Ollama. Error: Groq API returned error: 429
[12:35:17 INF] 🔄 Using local Ollama API (Model: llama3.1:8b)
[12:35:18 DBG] 📤 Sending request to Ollama API: http://localhost:11434/api/generate
[12:35:25 DBG] 📥 Received response from Ollama API. Length: 3124
[12:35:25 DBG] 🔍 Parsing AI response. Length: 3124
[12:35:25 INF] ✅ Successfully parsed resume. Name: John Doe
```

## Scenario 3: No Groq API Key, Using Ollama Only

```
[12:40:00 WRN] ⚠️ Groq API key not found. Will use Ollama only.
[12:40:05 INF] 🔄 Using local Ollama API (Model: llama3.1:8b)
[12:40:06 DBG] 📤 Sending request to Ollama API: http://localhost:11434/api/generate
[12:40:12 DBG] 📥 Received response from Ollama API. Length: 2987
[12:40:12 DBG] 🔍 Parsing AI response. Length: 2987
[12:40:12 INF] ✅ Successfully parsed resume. Name: John Doe
```

## Scenario 4: Chat Title Generation with Groq

```
[12:45:00 INF] 🚀 Generating chat title using Groq API
[12:45:01 INF] ✅ Successfully generated title using Groq API
```

## Scenario 5: Error Handling

```
[12:50:00 INF] 🚀 Attempting to enhance resume using Groq API (Model: llama-3.3-70b-versatile)
[12:50:01 ERR] ❌ Groq API error: 401 - Invalid API key
[12:50:01 WRN] ⚠️ Groq API failed. Falling back to local Ollama. Error: Groq API returned error: 401
[12:50:01 INF] 🔄 Using local Ollama API (Model: llama3.1:8b)
[12:50:02 ERR] ❌ Failed to connect to Ollama at http://localhost:11434. Make sure Ollama is running.
[12:50:02 ERR] System.Exception: Failed to connect to Ollama at http://localhost:11434. Please ensure Ollama is running.
```

## Log Level Meanings

| Level | Symbol | Meaning |
|-------|--------|---------|
| **INF** | ✅ 🚀 🔄 | Information - Normal operation |
| **WRN** | ⚠️ | Warning - Fallback triggered |
| **ERR** | ❌ | Error - Something failed |
| **DBG** | 📤 📥 🔍 | Debug - Detailed operation info |

## Emoji Legend

| Emoji | Meaning |
|-------|---------|
| ✅ | Success / Configured |
| 🚀 | Attempting Groq API |
| 🔄 | Using Ollama |
| ⚠️ | Warning / Fallback |
| ❌ | Error |
| 📤 | Sending request |
| 📥 | Receiving response |
| 🔍 | Parsing response |
| 📝 | Processing data |
