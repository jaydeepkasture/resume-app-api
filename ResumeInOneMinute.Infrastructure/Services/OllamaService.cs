using System.Text;
using System.Text.Json;
using ResumeInOneMinute.Domain.DTO;
using ResumeInOneMinute.Domain.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ResumeInOneMinute.Infrastructure.Services;

public class OllamaService : IOllamaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaService> _logger;
    private readonly string _ollamaUrl;
    private readonly string _model;

    public OllamaService(IConfiguration configuration, ILogger<OllamaService> logger)
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(5) // Ollama can take time for large prompts
        };
        _logger = logger;
        _ollamaUrl = configuration["OllamaSettings:Url"] ?? "http://localhost:11434";
        _model = configuration["OllamaSettings:Model"] ?? "deepseek-r1:8b";
    }

    public async Task<ResumeDto> EnhanceResumeAsync(ResumeDto originalResume, string enhancementInstruction)
    {
        try
        {
            var prompt = BuildPrompt(originalResume, enhancementInstruction);
            
            _logger.LogInformation("Calling Ollama API for resume enhancement");
            
            var ollamaRequest = new
            {
                model = _model,
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

            var response = await _httpClient.PostAsync($"{_ollamaUrl}/api/generate", content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Ollama API error: {response.StatusCode} - {errorContent}");
                throw new Exception($"Ollama API returned error: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseContent, options);

            if (ollamaResponse?.Response == null)
            {
                _logger.LogError("Ollama returned null response. Raw content: {Content}", responseContent);
                throw new Exception("Ollama returned empty response");
            }

            // Parse the enhanced resume from Ollama's response
            var enhancedResume = ParseResumeFromResponse(ollamaResponse.Response);
            
            _logger.LogInformation("Successfully enhanced resume using Ollama");
            
            return enhancedResume;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to Ollama. Make sure Ollama is running at {Url}", _ollamaUrl);
            throw new Exception($"Failed to connect to Ollama at {_ollamaUrl}. Please ensure Ollama is running. Install from https://ollama.com", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enhancing resume with Ollama");
            throw;
        }
    }

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

CRITICAL INSTRUCTIONS:
1. ANALYZE the enhancement instruction carefully. It may be a specific request (e.g., ""fix grammar"") or a JOB DESCRIPTION to tailor the resume for.
2. If a JOB DESCRIPTION is provided:
   - Tailor the Summary, Experience, and Skills to align with the job requirements.
   - You MUST update the ""summary"" to highlight relevant qualifications.
   - You MUST update ""experience"" details to emphasize relevant tasks and achievements.
   - You MAY ADD extra experience entries or bullet points if they are logically consistent with the candidate's profile and necessary for the job description.
   - Ensure the tone matches the industry (e.g., tech, corporate, creative).
3. If the instruction is in LAYMAN's language (e.g., ""make it better"", ""I want to apply for X""), interpret the intent professionally.
4. Use action verbs, quantifiable achievements, and industry-standard terminology.
5. Maintain the EXACT same JSON structure as the input.
6. Ensure all field names match exactly (case-sensitive): name, phoneno, email, location, linkedin, github, summary, experience, skills, education.
7. For experience array, use: company, position, from, to, description
8. For education array, use: degree, field, institution, year

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
            _logger.LogInformation("Parsing Ollama response. Length: {Length}", response.Length);
            
            // Remove markdown code blocks if present
            var cleanedResponse = response.Trim();
            
            // Remove ```json and ``` markers
            if (cleanedResponse.StartsWith("```json"))
            {
                cleanedResponse = cleanedResponse.Substring(7); // Remove ```json
            }
            else if (cleanedResponse.StartsWith("```"))
            {
                cleanedResponse = cleanedResponse.Substring(3); // Remove ```
            }
            
            if (cleanedResponse.EndsWith("```"))
            {
                cleanedResponse = cleanedResponse.Substring(0, cleanedResponse.Length - 3);
            }
            
            cleanedResponse = cleanedResponse.Trim();
            
            // Find JSON in the response (between first { and last })
            var startIndex = cleanedResponse.IndexOf('{');
            var endIndex = cleanedResponse.LastIndexOf('}');

            if (startIndex == -1 || endIndex == -1 || endIndex <= startIndex)
            {
                _logger.LogError("No valid JSON found in Ollama response. Response: {Response}", response);
                throw new Exception($"No valid JSON found in Ollama response. Response preview: {response.Substring(0, Math.Min(200, response.Length))}");
            }

            var jsonString = cleanedResponse.Substring(startIndex, endIndex - startIndex + 1);
            
            _logger.LogInformation("Extracted JSON. Length: {Length}", jsonString.Length);
            
            // Deserialize with case-insensitive property matching
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var enhancedResume = JsonSerializer.Deserialize<ResumeDto>(jsonString, options);

            if (enhancedResume == null)
            {
                _logger.LogError("Deserialization returned null. JSON: {Json}", jsonString.Substring(0, Math.Min(500, jsonString.Length)));
                throw new Exception("Failed to deserialize enhanced resume - result was null");
            }
            
            _logger.LogInformation("Successfully parsed resume. Name: {Name}", enhancedResume.Name);

            return enhancedResume;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing failed. Response length: {Length}, Error: {Error}", response.Length, ex.Message);
            _logger.LogError("Response preview (first 500 chars): {Preview}", response.Substring(0, Math.Min(500, response.Length)));
            throw new Exception($"Failed to parse enhanced resume from Ollama. JSON Error: {ex.Message}. Response preview: {response.Substring(0, Math.Min(200, response.Length))}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error parsing Ollama response");
            throw new Exception($"Failed to parse enhanced resume from Ollama: {ex.Message}", ex);
        }
    }

    public async Task<(string EnhancedHtml, ResumeDto EnhancedResume)> EnhanceResumeHtmlAsync(
        string resumeHtml, 
        ResumeDto resumeData, 
        string enhancementMessage)
    {
        _logger.LogInformation("Enhancing resume JSON only (HTML enhancement disabled). Instructions: {Instruction}", enhancementMessage);
        
        // Call the JSON-only enhancement method
        // This ensures stricter adherence to the data structure and avoids HTML parsing issues
        var enhancedResume = await EnhanceResumeAsync(resumeData, enhancementMessage);
        
        // Return original HTML (unchanged) and new JSON
        // The frontend should ideally re-render the HTML based on the new JSON if needed,
        // or the user accepts that only the data model is updated.
        return (resumeHtml, enhancedResume);
    }

    private class OllamaResponse
    {
        public string? Model { get; set; }
        public string? Response { get; set; }
        public bool Done { get; set; }
    }
}
