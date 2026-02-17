using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using ResumeInOneMinute.Domain.Interface;

namespace ResumeInOneMinute.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly bool _enableSsl;
    private readonly string _senderEmail;

    public SmtpEmailService(IConfiguration configuration)
    {
        _host = configuration["SmtpSettings:Host"] ?? throw new InvalidOperationException("SMTP Host is not configured properly in environment variables.");
        
        if (!int.TryParse(configuration["SmtpSettings:Port"], out var port))
        {
            _port = 587; // Default SMTP port
        }
        else
        {
            _port = port;
        }

        _username = configuration["SmtpSettings:Username"] ?? throw new InvalidOperationException("SMTP Username is not configured properly in environment variables.");
        _password = configuration["SmtpSettings:Password"] ?? throw new InvalidOperationException("SMTP Password is not configured properly in environment variables.");
        _enableSsl = bool.Parse(configuration["SmtpSettings:EnableSsl"] ?? "true");
        _senderEmail = _username; // Default to username as sender
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(_host, _port);
        client.Credentials = new NetworkCredential(_username, _password);
        client.EnableSsl = _enableSsl;

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_senderEmail, "1mincv.com"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };
        mailMessage.To.Add(to);

        try
        {
            await client.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Error sending email via SMTP: {ex.Message}");
            throw;
        }
    }
}
