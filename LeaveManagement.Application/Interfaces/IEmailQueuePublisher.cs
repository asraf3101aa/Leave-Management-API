using LeaveManagement.Application.Models.Email;

namespace LeaveManagement.Application.Interfaces;

public interface IEmailQueuePublisher
{
    Task PublishEmailAsync(EmailMessage email);
}
