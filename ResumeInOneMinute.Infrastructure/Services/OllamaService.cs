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
                prompt,
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

EXAMPLES OF UPDATE OPERATIONS:

Example 1 - Updating a specific company's description:
User says: ""Update OSP Labs details by adding details about .NET microservices""
Action: Find the experience entry where company contains ""OSP Labs"" (case-insensitive match).
Update the description field by adding information about .NET microservices to the existing description.
Result: The OSP Labs entry's description should now include microservices-related achievements.

Example 2 - Adding skills to a specific role:
User says: ""Add Angular and TypeScript to my NSR experience""
Action: Find the experience entry where company is ""NSR"".
Update the description to mention Angular and TypeScript work.
Optionally add ""Angular"" and ""TypeScript"" to the skills array if not already present.

Example 3 - Enhancing a specific company's achievements:
User says: ""Make the OSP Labs experience more impressive""
Action: Find the OSP Labs entry and rewrite the description with stronger action verbs, quantifiable metrics, and more impactful language.

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

    public async Task<string> GenerateChatTitleAsync(string instruction)
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
            
            _logger.LogInformation("Generating chat title via Ollama");
            
            var ollamaRequest = new
            {
                model = _model,
                prompt,
                stream = false,
                options = new
                {
                    temperature = 0.7, // Higher temperature for creativity
                    top_p = 0.9
                }
            };

            var jsonContent = JsonSerializer.Serialize(ollamaRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_ollamaUrl}/api/generate", content);
            
            if (!response.IsSuccessStatusCode)
            {
                 // Fallback if API fails
                 return instruction.Length > 30 ? instruction.Substring(0, 27) + "..." : instruction;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (string.IsNullOrWhiteSpace(ollamaResponse?.Response))
            {
                return instruction.Length > 30 ? instruction.Substring(0, 27) + "..." : instruction;
            }

            var title = ollamaResponse.Response.Trim();
            
            // Cleanup quotes if Ollama added them
            title = title.Trim('"').Trim('\'');
            
            // Enforce max length
            if (title.Length > 400)
            {
                title = title.Substring(0, 397) + "...";
            }
            
            return title;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chat title");
            // Fallback
            return instruction.Length > 30 ? instruction.Substring(0, 27) + "..." : instruction;
        }
    }

    private class OllamaResponse
    {
        public string? Model { get; set; }
        public string? Response { get; set; }
        public bool Done { get; set; }
    }
}
