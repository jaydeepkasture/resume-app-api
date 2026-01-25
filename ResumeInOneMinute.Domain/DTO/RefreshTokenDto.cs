namespace ResumeInOneMinute.Domain.DTO;

public class RefreshTokenDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
