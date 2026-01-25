namespace ResumeInOneMinute.Domain.DTO;

public class UserDto
{
    public long UserId { get; set; }
    public Guid GlobalUserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

