using System.Text;
using System.Text.Json;
using api.Models.Job;
using RabbitMQ.Client;

namespace api.Services.Job;

public class JobService(RabbitMqConnection rabbitMqConnection)
{
  public async Task QueueTransactionImport(
    TransactionImportJobPayload payload,
    CancellationToken cancellationToken = default
  )
  {
    var json = JsonSerializer.Serialize(payload);
    var body = Encoding.UTF8.GetBytes(json);
    var properties = new BasicProperties
    {
      ContentType = "application/json",
      DeliveryMode = DeliveryModes.Persistent,
      MessageId = payload.JobId.ToString(),
      Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
    };

    await using var channel = await rabbitMqConnection.CreateChannelAsync(cancellationToken);

    await channel.BasicPublishAsync(
      exchange: string.Empty,
      routingKey: rabbitMqConnection.TransactionImportQueueName,
      mandatory: true,
      basicProperties: properties,
      body: body,
      cancellationToken: cancellationToken
    );
  }
}
