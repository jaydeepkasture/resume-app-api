using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ResumeInOneMinute.Infrastructure.Services;

public class GroqService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GroqService> _logger;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public GroqService(IConfiguration configuration, ILogger<GroqService> logger)
    {
        _logger = logger;
        _apiKey = configuration["GroqSettings:ApiKey"] 
                  ?? Environment.GetEnvironmentVariable("GROQ_API_KEY") 
                  ?? throw new InvalidOperationException("GROQ_API_KEY is not configured");
        
        _baseUrl = configuration["GroqSettings:BaseUrl"] ?? "https://api.groq.com/openai/v1";

        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(2)
        };
        
        // Set authorization header
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> CreateResponseAsync(string input, string model = "openai/gpt-oss-20b")
    {
        try
        {
            _logger.LogInformation("Calling Groq API with model: {Model}", model);

            var requestBody = new
            {
                input = input,
                model = model
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/responses", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Groq API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                throw new Exception($"Groq API returned error: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var groqResponse = JsonSerializer.Deserialize<GroqResponse>(responseContent, options);

            if (string.IsNullOrWhiteSpace(groqResponse?.OutputText))
            {
                _logger.LogError("Groq returned null or empty output_text. Raw content: {Content}", responseContent);
                throw new Exception("Groq returned empty response");
            }

            _logger.LogInformation("Successfully received response from Groq");
            return groqResponse.OutputText;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to Groq API at {BaseUrl}", _baseUrl);
            throw new Exception($"Failed to connect to Groq API at {_baseUrl}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Groq API");
            throw;
        }
    }

    private class GroqResponse
    {
        public string? OutputText { get; set; }
        public string? Model { get; set; }
        public int? TokensUsed { get; set; }
    }
}
