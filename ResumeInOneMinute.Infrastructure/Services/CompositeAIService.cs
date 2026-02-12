using System.Text;
using System.Text.Json;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ResumeInOneMinute.Infrastructure.Services;

/// <summary>
/// Composite AI Service that tries Groq API first, then falls back to local Ollama
/// </summary>
public class CompositeAIService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CompositeAIService> _logger;
    private readonly string? _groqApiKey;
    private readonly string _groqBaseUrl;
    private readonly string _groqModel;
    private readonly string _ollamaUrl;
    private readonly string _ollamaModel;
    private readonly bool _useGroq;

    public CompositeAIService(IConfiguration configuration, ILogger<CompositeAIService> logger)
    {
        _logger = logger;
        
        // Groq Configuration
        _groqApiKey = configuration["GroqSettings:ApiKey"];

        _groqBaseUrl = configuration["GroqSettings:BaseUrl"] ?? "https://api.groq.com/openai/v1";
        _groqModel = configuration["GroqSettings:Model"] ?? "llama-3.3-70b-versatile";
        
        // Ollama Configuration
        _ollamaUrl = configuration["OllamaSettings:Url"] ?? "http://localhost:11434";
        _ollamaModel = configuration["OllamaSettings:Model"] ?? "deepseek-r1:8b";
        
        // Determine if Groq should be used
        _useGroq = !string.IsNullOrWhiteSpace(_groqApiKey);
        
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(5)
        };
        
        if (_useGroq)
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_groqApiKey}");
            _logger.LogInformation("✅ Groq API configured. Will use Groq as primary AI service with Ollama as fallback.");
        }
        else
        {
            _logger.LogWarning("⚠️ Groq API key not found. Will use Ollama only.");
        }
    }

    public async Task<ResumeDto> EnhanceResumeAsync(ResumeDto originalResume, string enhancementInstruction)
    {
        // Try Groq first if configured
        if (_useGroq)
        {
            try
            {
                _logger.LogInformation("🚀 Attempting to enhance resume using Groq API (Model: {Model})", _groqModel);
                var result = await EnhanceResumeWithGroqAsync(originalResume, enhancementInstruction);
                _logger.LogInformation("✅ Successfully enhanced resume using Groq API");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Groq API failed. Falling back to local Ollama. Error: {Message}", ex.Message);
            }
        }

        // Fallback to Ollama
        _logger.LogInformation("🔄 Using local Ollama API (Model: {Model})", _ollamaModel);
        return await EnhanceResumeWithOllamaAsync(originalResume, enhancementInstruction);
    }

    public async Task<(string EnhancedHtml, ResumeDto EnhancedResume)> EnhanceResumeHtmlAsync(
        string resumeHtml, 
        ResumeDto resumeData, 
        string enhancementMessage)
    {
        _logger.LogInformation("📝 Enhancing resume JSON only (HTML enhancement disabled). Instructions: {Instruction}", enhancementMessage);
        
        var enhancedResume = await EnhanceResumeAsync(resumeData, enhancementMessage);
        
        return (resumeHtml, enhancedResume);
    }

    public async Task<string> GenerateChatTitleAsync(string instruction)
    {
        // Try Groq first if configured
        if (_useGroq)
        {
            try
            {
                _logger.LogInformation("🚀 Generating chat title using Groq API");
                var result = await GenerateTitleWithGroqAsync(instruction);
                _logger.LogInformation("✅ Successfully generated title using Groq API");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Groq API failed for title generation. Falling back to Ollama. Error: {Message}", ex.Message);
            }
        }

        // Fallback to Ollama
        _logger.LogInformation("🔄 Generating title using local Ollama API");
        return await GenerateTitleWithOllamaAsync(instruction);
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

        _logger.LogDebug("📤 Sending request to Groq API: {Url}/chat/completions", _groqBaseUrl);
        var response = await _httpClient.PostAsync($"{_groqBaseUrl}/chat/completions", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("❌ Groq API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
            throw new Exception($"Groq API returned error: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("📥 Received response from Groq API. Length: {Length}", responseContent.Length);
        
        var groqResponse = JsonSerializer.Deserialize<GroqChatResponse>(responseContent, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        var aiResponse = groqResponse?.Choices?[0]?.Message?.Content;
        
        if (string.IsNullOrWhiteSpace(aiResponse))
        {
            _logger.LogError("❌ Groq returned null or empty response");
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
        
        // Cleanup
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

    #region Ollama Implementation

    private async Task<ResumeDto> EnhanceResumeWithOllamaAsync(ResumeDto originalResume, string enhancementInstruction)
    {
        try
        {
            var prompt = BuildPrompt(originalResume, enhancementInstruction);
            
            var ollamaRequest = new
            {
                model = _ollamaModel,
                prompt = prompt,
                stream = false,
                options = new
                {
                    temperature = 0.3,
                    top_p = 0.9
                }
            };

            var jsonContent = JsonSerializer.Serialize(ollamaRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogDebug("📤 Sending request to Ollama API: {Url}/api/generate", _ollamaUrl);
            var response = await _httpClient.PostAsync($"{_ollamaUrl}/api/generate", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ Ollama API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new Exception($"Ollama API returned error: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("📥 Received response from Ollama API. Length: {Length}", responseContent.Length);
            
            var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (ollamaResponse?.Response == null)
            {
                _logger.LogError("❌ Ollama returned null response");
                throw new Exception("Ollama returned empty response");
            }

            return ParseResumeFromResponse(ollamaResponse.Response);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "❌ Failed to connect to Ollama at {Url}. Make sure Ollama is running.", _ollamaUrl);
            throw new Exception($"Failed to connect to Ollama at {_ollamaUrl}. Please ensure Ollama is running.", ex);
        }
    }

    private async Task<string> GenerateTitleWithOllamaAsync(string instruction)
    {
        try
        {
            var prompt = $@"Generate a short, concise title (max 5-7 words) for a chat session based on this user instruction: ""{instruction}"".
            
RULES:
1. Return ONLY the title text.
2. Do NOT use quotes.
3. Do NOT include any prefixes like ""Title:"".
4. Keep it professional and descriptive.
5. Max length 400 characters.

Title:";
            
            var ollamaRequest = new
            {
                model = _ollamaModel,
                prompt = prompt,
                stream = false,
                options = new
                {
                    temperature = 0.7,
                    top_p = 0.9
                }
            };

            var jsonContent = JsonSerializer.Serialize(ollamaRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_ollamaUrl}/api/generate", content);
            
            if (!response.IsSuccessStatusCode)
            {
                return instruction.Length > 30 ? instruction.Substring(0, 27) + "..." : instruction;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseContent, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            if (string.IsNullOrWhiteSpace(ollamaResponse?.Response))
            {
                return instruction.Length > 30 ? instruction.Substring(0, 27) + "..." : instruction;
            }

            var title = ollamaResponse.Response.Trim();
            title = title.Trim('"').Trim('\'');
            
            if (title.Length > 400)
            {
                title = title.Substring(0, 397) + "...";
            }
            
            return title;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating chat title with Ollama");
            return instruction.Length > 30 ? instruction.Substring(0, 27) + "..." : instruction;
        }
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
1. ANALYZE the enhancement instruction carefully. It may be a specific request (e.g., ""fix grammar"") or a JOB DESCRIPTION to tailor the resume for.
2. If a JOB DESCRIPTION is provided:
   - Tailor the Summary, Experience, and Skills to align with the job requirements.
   - You MUST update the ""summary"" to highlight relevant qualifications.
   - You MUST update ""experience"" details to emphasize relevant tasks and achievements.
   - You MAY ADD extra experience entries or bullet points if they are logically consistent with the candidate's profile and necessary for the job description.
   - Ensure the tone matches the industry (e.g., tech, corporate, creative).
3. If the user instruction is to UPDATE/MODIFY content (e.g., ""update OSP Labs details"", ""add microservices to NSR experience"", ""enhance company X description""):
   - IDENTIFY the specific company/entry mentioned in the instruction by matching the company name (case-insensitive, partial match allowed).
   - LOCATE that entry in the experience/education array.
   - UPDATE the relevant fields (description, position, dates, etc.) based on the instruction.
   - If adding new details to a description, APPEND or INTEGRATE them naturally into the existing description.
   - PRESERVE all other fields of that entry unless explicitly instructed to change them.
   - DO NOT create a duplicate entry - modify the existing one.
4. If the user instruction is to ADD content (e.g., ""add education"", ""add experience"", ""add skill""):
   - You MUST append the new entry to the appropriate array.
   - If specific details are provided (e.g., ""add university X"", ""add skill Y""), use them.
   - If details are generic, infer reasonable placeholders or structure based on the context.
5. If the user instruction is to REMOVE or CONSOLIDATE content (e.g., ""remove duplicate entries"", ""consolidate experience""):
   - Identify duplicate or overlapping entries (same company, position, or time period).
   - MERGE them into a single comprehensive entry or remove the redundant ones.
   - Adjust dates and descriptions to reflect the consolidated timeline.
6. If the instruction is in LAYMAN's language (e.g., ""make it better"", ""I want to apply for X"", ""add details in edution"", ""remove dupliate""), interpret the intent and typos intelligently.
7. Use action verbs, quantifiable achievements, and industry-standard terminology.
8. Maintain the EXACT same JSON structure as the input.
9. Ensure all field names match exactly (case-sensitive): name, role, phoneno, email, location, linkedin, github, summary, experience, skills, education.
10. For experience array, use: company, position, from, to, description
11. For education array, use: degree, field, institution, year
12. EXPERIENCE ORDER: You MUST maintain strict reverse chronological order for the 'experience' array. The most recent or current job MUST be at index 0. The oldest (first) job MUST be at the last index.
13. EDUCATION YEAR LOGIC: If the job description requires specific education and you are adding or inferring graduation years, set the graduation year to be the same as the start year of the candidate's very first job (the oldest entry in their experience history).

OUTPUT FORMAT:
CRITICAL: Return ONLY the raw JSON object. Do NOT wrap it in markdown code blocks.
Do NOT include ```json or ``` markers.
Do NOT add any explanatory text before or after the JSON.
Return ONLY a valid JSON object with the EXACT structure shown below.
- NO markdown code blocks (no ```json)
- NO comments
- NO additional text before or after the JSON
- Ensure all strings are properly escaped
- No trailing commas
- Use exact field names as shown

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
            _logger.LogDebug("🔍 Parsing AI response. Length: {Length}", response.Length);
            
            var cleanedResponse = response.Trim();
            
            // Remove markdown code blocks if present
            if (cleanedResponse.StartsWith("```json"))
            {
                cleanedResponse = cleanedResponse.Substring(7);
            }
            else if (cleanedResponse.StartsWith("```"))
            {
                cleanedResponse = cleanedResponse.Substring(3);
            }
            
            if (cleanedResponse.EndsWith("```"))
            {
                cleanedResponse = cleanedResponse.Substring(0, cleanedResponse.Length - 3);
            }
            
            cleanedResponse = cleanedResponse.Trim();
            
            // Find JSON in the response
            var startIndex = cleanedResponse.IndexOf('{');
            var endIndex = cleanedResponse.LastIndexOf('}');

            if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
            {
                _logger.LogError("❌ No valid JSON found in AI response");
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
                _logger.LogError("❌ Deserialization returned null");
                throw new Exception("Failed to deserialize enhanced resume");
            }
            
            _logger.LogInformation("✅ Successfully parsed resume. Name: {Name}", enhancedResume.Name);
            return enhancedResume;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "❌ JSON parsing failed: {Error}", ex.Message);
            throw new Exception($"Failed to parse enhanced resume: {ex.Message}", ex);
        }
    }

    #endregion

    #region Response Models

    private class GroqChatResponse
    {
        public string? Id { get; set; }
        public string? Object { get; set; }
        public long Created { get; set; }
        public string? Model { get; set; }
        public Choice[]? Choices { get; set; }
        public Usage? Usage { get; set; }
    }

    private class Choice
    {
        public int Index { get; set; }
        public Message? Message { get; set; }
        public string? FinishReason { get; set; }
    }

    private class Message
    {
        public string? Role { get; set; }
        public string? Content { get; set; }
    }

    private class Usage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }

    private class OllamaResponse
    {
        public string? Model { get; set; }
        public string? Response { get; set; }
        public bool Done { get; set; }
    }

    #endregion
}
