using System.ComponentModel.DataAnnotations;

namespace ResumeInOneMinute.Domain.DTO;

public class ProfileUpdateDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(15)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(5)]
    public string? CountryCode { get; set; }
}
