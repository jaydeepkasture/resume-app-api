using ResumeInOneMinute.Domain.Interface;

namespace ResumeInOneMinute.Infrastructure.Services;

public class EmailService : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body)
    {
        // For now, we log the email to the console/debug output.
        // In a real application, you would use SmtpClient or a service like userGrid/Mailgun.
        System.Diagnostics.Debug.WriteLine($"[EmailService] To: {to}");
        System.Diagnostics.Debug.WriteLine($"[EmailService] Subject: {subject}");
        System.Diagnostics.Debug.WriteLine($"[EmailService] Body: {body}");
        Console.WriteLine($"[EmailService] To: {to}, Subject: {subject}, Body: {body}");

        return Task.CompletedTask;
    }
}
