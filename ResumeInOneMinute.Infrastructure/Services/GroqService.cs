using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;

namespace ResumeInOneMinute.Infrastructure.Services;

/// <summary>
/// Groq Service for resume extraction with configurable retries.
/// </summary>
public class GroqService : IGroqService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GroqService> _logger;
    private readonly string _baseUrl;
    private readonly string _textModel;
    private readonly string _visionModel;
    private readonly int _retryCount;

    public GroqService(IConfiguration configuration, ILogger<GroqService> logger)
    {
        _logger = logger;
        var apiKey = configuration["GroqSettings:ApiKey"]
                  ?? throw new InvalidOperationException("GroqSettings:ApiKey is not configured");

        _baseUrl = configuration["GroqSettings:BaseUrl"] ?? "https://api.groq.com/openai/v1";
        _textModel = configuration["GroqSettings:Model"] ?? "llama-3.3-70b-versatile"; 
        _visionModel = configuration["GroqSettings:VisionModel"] ?? "llama-3.2-90b-vision-preview"; 

        if (!int.TryParse(configuration["GroqSettings:RetryCount"], out _retryCount))
        {
            _retryCount = 3;
        }

        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(2)
        };
        
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<ResumeDto> ExtractResumeFromTextAsync(string text)
    {
        return await ExecuteWithRetryAsync(async () => 
        {
            var prompt = BuildExtractionPrompt(text);
            return await CallGroqApiAsync<ResumeDto>(prompt, _textModel);
        });
    }
    
    public async Task<ResumeDto> ExtractResumeFromImageAsync(string base64Image)
    {
        return await ExecuteWithRetryAsync(async () => 
        {
            var prompt = BuildExtractionPrompt("Extract resume information from the image.");
            return await CallGroqVisionApiAsync<ResumeDto>(base64Image, prompt, _visionModel);
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
                    _logger.LogInformation("🔄 GroqService retry attempt {Attempt}/{MaxCount}...", attempts + 1, _retryCount);
                }
                return await action();
            }
            catch (Exception ex)
            {
                attempts++;
                lastException = ex;
                _logger.LogWarning(ex, "⚠️ GroqService attempt {Attempt} failed: {Message}", attempts, ex.Message);
                
                if (attempts < _retryCount)
                {
                    int delayMs = (int)Math.Pow(2, attempts - 1) * 1000;
                    await Task.Delay(delayMs);
                }
            }
        }

        _logger.LogError(lastException, "❌ All {MaxCount} attempts failed in GroqService.", _retryCount);
        throw new Exception("AI service is temporarily unavailable. Please try again later.");
    }

    private string BuildExtractionPrompt(string content)
    {
        return $@"You are an expert resume parser. Your job is to extract resume data from the provided text or image and format it into a specific JSON structure.

SOURCE CONTENT:
{content}

INSTRUCTIONS:
1. Extract the candidate's name, role (current or target), contact info, location.
2. Extract Summary/Profile.
3. Extract Experience: Company, Position, Dates (From/To), Description. Use reverse chronological order.
4. Extract Education: Degree, Field, Institution, Year.
5. Extract Skills as a list of strings.
6. Return VALID JSON only.

REQUIRED JSON FORMAT:
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
}}";
    }

    private async Task<T> CallGroqApiAsync<T>(string prompt, string model)
    {
         var requestBody = new
        {
            model = model,
            messages = new[]
            {
                new { role = "system", content = "You are a helpful assistant that outputs only valid JSON." },
                new { role = "user", content = prompt }
            },
            temperature = 0.1,
            response_format = new { type = "json_object" }
        };

        return await SendRequestAsync<T>(requestBody);
    }

    private async Task<T> CallGroqVisionApiAsync<T>(string base64Image, string prompt, string model)
    {
        var requestBody = new
        {
            model = model,
            messages = new object[]
            {
                new 
                { 
                    role = "user", 
                    content = new object[] 
                    {
                        new { type = "text", text = prompt },
                        new 
                        { 
                            type = "image_url", 
                            image_url = new { url = $"data:image/jpeg;base64,{base64Image}" } 
                        }
                    }
                }
            },
            temperature = 0.1
        };

        return await SendRequestAsync<T>(requestBody);
    }

    private async Task<T> SendRequestAsync<T>(object requestBody)
    {
        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/chat/completions", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Groq API error: {StatusCode} - {Error}", response.StatusCode, error);
            throw new Exception($"Groq API returned error: {response.StatusCode}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var groqResponse = JsonSerializer.Deserialize<GroqChatResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        var jsonResult = groqResponse?.Choices?[0]?.Message?.Content;
        if (string.IsNullOrWhiteSpace(jsonResult)) throw new Exception("Empty response from Groq");

        if (jsonResult.Contains("```json"))
        {
            jsonResult = jsonResult.Replace("```json", "").Replace("```", "").Trim();
        }
        else if (jsonResult.Contains("```"))
        {
            jsonResult = jsonResult.Replace("```", "").Trim();
        }

        return JsonSerializer.Deserialize<T>(jsonResult, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

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
}
