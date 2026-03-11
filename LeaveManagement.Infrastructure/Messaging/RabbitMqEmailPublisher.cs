using System.Text;
using System.Text.Json;
using LeaveManagement.Application.Interfaces;
using LeaveManagement.Application.Models.Email;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace LeaveManagement.Infrastructure.Messaging;

public class RabbitMqEmailPublisher : IEmailQueuePublisher
{
    private readonly string _hostname;
    private readonly string _queueName;

    public RabbitMqEmailPublisher(IConfiguration configuration)
    {
        _hostname = configuration["RabbitMq:Host"] ?? "localhost";
        _queueName = configuration["RabbitMq:EmailQueueName"] ?? "email_queue";
    }

    public async Task PublishEmailAsync(EmailMessage email)
    {
        var factory = new ConnectionFactory { HostName = _hostname };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var message = JsonSerializer.Serialize(email);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = new BasicProperties
        {
            Persistent = true
        };

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: _queueName,
            mandatory: true,
            basicProperties: properties,
            body: body
        );
    }
}
