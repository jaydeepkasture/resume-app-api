using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Configuration;
using ResumeInOneMinute.Domain.Interface;

namespace ResumeInOneMinute.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly string _region;
    private readonly string _accessKey;
    private readonly string _secretKey;
    private readonly string _senderEmail;

    public EmailService(IConfiguration configuration)
    {
        _region = configuration["AwsSettings:Region"] ?? throw new InvalidOperationException("AWS SES Region is not configured in environment variables.");
        _accessKey = configuration["AwsSettings:AccessKey"] ?? throw new InvalidOperationException("AWS SES AccessKey is not configured in environment variables.");
        _secretKey = configuration["AwsSettings:SecretKey"] ?? throw new InvalidOperationException("AWS SES SecretKey is not configured in environment variables.");
        _senderEmail = configuration["AwsSettings:SenderEmail"] ?? throw new InvalidOperationException("AWS SES SenderEmail is not configured in environment variables.");
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var regionEndpoint = RegionEndpoint.GetBySystemName(_region);
        using var client = new AmazonSimpleEmailServiceClient(_accessKey, _secretKey, regionEndpoint);

        var sendRequest = new SendEmailRequest
        {
            Source = _senderEmail,
            Destination = new Destination
            {
                ToAddresses = new List<string> { to }
            },
            Message = new Message
            {
                Subject = new Content(subject),
                Body = new Body
                {
                    Html = new Content
                    {
                        Charset = "UTF-8",
                        Data = body
                    }
                }
            }
        };

        try
        {
            await client.SendEmailAsync(sendRequest);
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Error sending email via AWS SES: {ex.Message}");
            throw;
        }
    }
}
