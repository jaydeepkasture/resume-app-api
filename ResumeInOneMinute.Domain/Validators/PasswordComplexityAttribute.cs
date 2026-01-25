using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ResumeInOneMinute.Domain.Validators;

/// <summary>
/// Validates password complexity requirements
/// </summary>
public class PasswordComplexityAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return false;

        var password = value.ToString()!;

        // Check for at least one uppercase letter
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            ErrorMessage = "Password must contain at least one uppercase letter";
            return false;
        }

        // Check for at least one lowercase letter
        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            ErrorMessage = "Password must contain at least one lowercase letter";
            return false;
        }

        // Check for at least one symbol/special character
        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
        {
            ErrorMessage = "Password must contain at least one symbol (!@#$%^&*()_+-=[]{}; etc.)";
            return false;
        }

        return true;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (IsValid(value))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(ErrorMessage ?? "Password does not meet complexity requirements");
    }
}
