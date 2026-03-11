using LeaveManagement.Application.Models.Email;

namespace LeaveManagement.Application.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(EmailMessage email);
}
