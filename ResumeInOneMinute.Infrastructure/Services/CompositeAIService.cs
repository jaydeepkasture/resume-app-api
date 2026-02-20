using System.Text;
using System.Text.Json;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ResumeInOneMinute.Infrastructure.Services;

/// <summary>
/// AI Service that uses Groq API with configurable retries.
/// Ollama fallback has been removed as per user request.
/// </summary>
public class CompositeAIService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CompositeAIService> _logger;
    private readonly string? _groqApiKey;
    private readonly string _groqBaseUrl;
    private readonly string _groqModel;
    private readonly int _retryCount;

    public CompositeAIService(IConfiguration configuration, ILogger<CompositeAIService> logger)
    {
        _logger = logger;
        
        // Groq Configuration
        _groqApiKey = configuration["GroqSettings:ApiKey"];
        _groqBaseUrl = configuration["GroqSettings:BaseUrl"] ?? "https://api.groq.com/openai/v1";
        _groqModel = configuration["GroqSettings:Model"] ?? "llama-3.3-70b-versatile";
        
        // Retry logic configuration
        if (!int.TryParse(configuration["GroqSettings:RetryCount"], out _retryCount))
        {
            _retryCount = 3; // Default to 3 retries
        }
        
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(2) // Reduced timeout since we have retries
        };
        
        if (!string.IsNullOrWhiteSpace(_groqApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_groqApiKey}");
            _logger.LogInformation("✅ Groq API configured with {RetryCount} retries. Model: {Model}", _retryCount, _groqModel);
        }
        else
        {
            _logger.LogCritical("❌ Groq API key not found. AI services will fail.");
        }
    }

    public async Task<ResumeDto> EnhanceResumeAsync(ResumeDto originalResume, string enhancementInstruction)
    {
        return await ExecuteWithRetryAsync(async () => 
        {
            _logger.LogInformation("🚀 Attempting to enhance resume using Groq API");
            return await EnhanceResumeWithGroqAsync(originalResume, enhancementInstruction);
        });
    }

    public async Task<(string EnhancedHtml, ResumeDto EnhancedResume)> EnhanceResumeHtmlAsync(
        string resumeHtml, 
        ResumeDto resumeData, 
        string enhancementMessage)
    {
        // Currently we only enhance the JSON data. HTML generation is handled by the template based on enhanced JSON.
        _logger.LogInformation("📝 Enhancing resume JSON. Instructions: {Instruction}", enhancementMessage);
        
        var enhancedResume = await EnhanceResumeAsync(resumeData, enhancementMessage);
        
        return (resumeHtml, enhancedResume);
    }

    public async Task<string> GenerateChatTitleAsync(string instruction)
    {
        return await ExecuteWithRetryAsync(async () => 
        {
            _logger.LogInformation("🚀 Generating chat title using Groq API");
            return await GenerateTitleWithGroqAsync(instruction);
        });
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action)
    {
        int attempts = 0;
        Exception? lastException = null;

        while (attempts < _retryCount)
        {
            try
            {
                if (attempts > 0)
                {
                    _logger.LogInformation("🔄 Retry attempt {Attempt}/{MaxCount}...", attempts + 1, _retryCount);
                }
                return await action();
            }
            catch (Exception ex)
            {
                attempts++;
                lastException = ex;
                _logger.LogWarning(ex, "⚠️ Attempt {Attempt} failed: {Message}", attempts, ex.Message);
                
                if (attempts < _retryCount)
                {
                    // Exponential backoff: 1s, 2s, 4s...
                    int delayMs = (int)Math.Pow(2, attempts - 1) * 1000;
                    await Task.Delay(delayMs);
                }
            }
        }

        _logger.LogError(lastException, "❌ All {MaxCount} attempts failed to execute AI service call.", _retryCount);
        throw new Exception("AI service is temporarily unavailable. Please try again later.");
    }

    #region Groq Implementation

    private async Task<ResumeDto> EnhanceResumeWithGroqAsync(ResumeDto originalResume, string enhancementInstruction)
    {
        var prompt = BuildPrompt(originalResume, enhancementInstruction);
        
        var requestBody = new
        {
            model = _groqModel,
            messages = new[]
            {
                new { role = "system", content = "You are an expert resume writer and career consultant." },
                new { role = "user", content = prompt }
            },
            temperature = 0.3,
            max_tokens = 4000
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_groqBaseUrl}/chat/completions", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("❌ Groq API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
            throw new Exception($"Groq API returned error: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        
        var groqResponse = JsonSerializer.Deserialize<GroqChatResponse>(responseContent, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        var aiResponse = groqResponse?.Choices?[0]?.Message?.Content;
        
        if (string.IsNullOrWhiteSpace(aiResponse))
        {
            throw new Exception("Groq returned empty response");
        }

        return ParseResumeFromResponse(aiResponse);
    }

    private async Task<string> GenerateTitleWithGroqAsync(string instruction)
    {
        var prompt = $@"Generate a short, concise title (max 5-7 words) for a chat session based on this user instruction: ""{instruction}"".
            
RULES:
1. Return ONLY the title text.
2. Do NOT use quotes.
3. Do NOT include any prefixes like ""Title:"".
4. Keep it professional and descriptive.
5. Max length 400 characters.

Title:";

        var requestBody = new
        {
            model = _groqModel,
            messages = new[]
            {
                new { role = "system", content = "You are a helpful assistant that generates concise chat titles." },
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 50
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_groqBaseUrl}/chat/completions", content);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Groq API returned error: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var groqResponse = JsonSerializer.Deserialize<GroqChatResponse>(responseContent, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        var title = groqResponse?.Choices?[0]?.Message?.Content?.Trim() ?? "";
        
        title = title.Trim('"').Trim('\'');
        if (title.Length > 400)
        {
            title = title.Substring(0, 397) + "...";
        }

        return string.IsNullOrWhiteSpace(title) 
            ? (instruction.Length > 30 ? instruction.Substring(0, 27) + "..." : instruction)
            : title;
    }

    #endregion

    #region Shared Methods

    private string BuildPrompt(ResumeDto resume, string enhancementInstruction)
    {
        var resumeJson = JsonSerializer.Serialize(resume, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return $@"You are an expert resume writer and career consultant. You will receive a resume in JSON format and specific instructions for enhancement.

CURRENT RESUME (JSON):
{resumeJson}

ENHANCEMENT INSTRUCTION:
{enhancementInstruction}

⚠️ CRITICAL WARNING ⚠️
You MUST make changes to the resume based on the enhancement instruction above.
Returning the exact same resume without any modifications is NOT acceptable.
If the instruction asks to update, add, or modify something, you MUST do it.

CRITICAL INSTRUCTIONS:
1. ANALYZE the enhancement instruction carefully. It may be a specific request or a JOB DESCRIPTION to tailor the resume for.
2. If a JOB DESCRIPTION is provided: Tailor Summary, Experience, and Skills to align with requirements.
3. If instruction is to UPDATE/MODIFY content: Identify specific entries and update relevant fields.
4. If instruction is to ADD content: Append new entries to appropriate arrays.
5. If instruction is to REMOVE/CONSOLIDATE: Identify duplicates and merge them.
6. Use action verbs and quantifiable achievements.
7. Maintain EXACT JSON structure and field names (case-sensitive).
8. Experience MUST be in reverse chronological order (most recent at index 0).

OUTPUT FORMAT:
Return ONLY raw JSON object. NO markdown code blocks, NO text before/after.

{{
  ""name"": ""string"",
  ""role"": ""string"",
  ""phoneno"": ""string"",
  ""email"": ""string"",
  ""location"": ""string"",
  ""linkedin"": ""string"",
  ""github"": ""string"",
  ""summary"": ""string"",
  ""experience"": [
    {{
      ""company"": ""string"",
      ""position"": ""string"",
      ""from"": ""string"",
      ""to"": ""string"",
      ""description"": ""string""
    }}
  ],
  ""skills"": [""string""],
  ""education"": [
    {{
      ""degree"": ""string"",
      ""field"": ""string"",
      ""institution"": ""string"",
      ""year"": ""string""
    }}
  ]
}}

Now enhance the resume:";
    }

    private ResumeDto ParseResumeFromResponse(string response)
    {
        try
        {
            var cleanedResponse = response.Trim();
            
            if (cleanedResponse.StartsWith("```json")) cleanedResponse = cleanedResponse.Substring(7);
            else if (cleanedResponse.StartsWith("```")) cleanedResponse = cleanedResponse.Substring(3);
            
            if (cleanedResponse.EndsWith("```")) cleanedResponse = cleanedResponse.Substring(0, cleanedResponse.Length - 3);
            
            cleanedResponse = cleanedResponse.Trim();
            
            var startIndex = cleanedResponse.IndexOf('{');
            var endIndex = cleanedResponse.LastIndexOf('}');

            if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
            {
                throw new Exception("No valid JSON found in AI response");
            }

            var jsonString = cleanedResponse.Substring(startIndex, endIndex - startIndex + 1);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var enhancedResume = JsonSerializer.Deserialize<ResumeDto>(jsonString, options);

            if (enhancedResume == null)
            {
                throw new Exception("Failed to deserialize enhanced resume");
            }
            
            return enhancedResume;
        }
        catch (JsonException ex)
        {
            throw new Exception($"Failed to parse enhanced resume: {ex.Message}", ex);
        }
    }

    #endregion

    #region Response Models

    private class GroqChatResponse
    {
        public Choice[]? Choices { get; set; }
    }

    private class Choice
    {
        public Message? Message { get; set; }
    }

    private class Message
    {
        public string? Content { get; set; }
    }

    #endregion
}
