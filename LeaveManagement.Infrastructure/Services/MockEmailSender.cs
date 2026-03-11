using LeaveManagement.Application.Interfaces;
using LeaveManagement.Application.Models.Email;
using Microsoft.Extensions.Logging;

namespace LeaveManagement.Infrastructure.Services;

public class MockEmailSender : IEmailSender
{
    private readonly ILogger<MockEmailSender> _logger;

    public MockEmailSender(ILogger<MockEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(EmailMessage email)
    {
        _logger.LogInformation("----------------------------------------------");
        _logger.LogInformation("EMAIL SENT:");
        _logger.LogInformation("To: {To}", email.To);
        _logger.LogInformation("Subject: {Subject}", email.Subject);
        _logger.LogInformation("Body: {Body}", email.Body);
        _logger.LogInformation("----------------------------------------------");

        return Task.CompletedTask;
    }
}
