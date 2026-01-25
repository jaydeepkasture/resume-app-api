using System.ComponentModel.DataAnnotations;
using ResumeInOneMinute.Domain.Validators;

namespace ResumeInOneMinute.Domain.DTO;

public class RegisterDto
{
    [Required(ErrorMessage = "First name is required")]
    [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
    [RegularExpression(@"^\+?[1-9]\d{1,14}$", ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    [PasswordComplexity]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm password is required")]
    [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
