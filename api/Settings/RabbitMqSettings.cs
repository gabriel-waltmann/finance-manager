namespace api.Settings;

public class RabbitMqSettings
{
  public required string Host { get; set; }
  public required int Port { get; set; }
  public required string UserName { get; set; }
  public required string Password { get; set; }
  public required string TransactionImportQueueName { get; set; }
}
