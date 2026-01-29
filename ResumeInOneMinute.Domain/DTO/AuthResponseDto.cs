using System.Text.Json.Serialization;

namespace ResumeInOneMinute.Domain.DTO;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;

    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}
