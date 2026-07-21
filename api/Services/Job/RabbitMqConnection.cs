using api.Settings;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace api.Services.Job;

public sealed class RabbitMqConnection(IOptions<RabbitMqSettings> options) : IAsyncDisposable
{
  private readonly RabbitMqSettings _settings = options.Value;
  private readonly SemaphoreSlim _connectionLock = new(1, 1);
  private IConnection? _connection;

  public string TransactionImportQueueName => _settings.TransactionImportQueueName;

  public async Task<IChannel> CreateChannelAsync(CancellationToken cancellationToken = default)
  {
    var connection = await GetConnectionAsync(cancellationToken);
    var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

    await channel.QueueDeclareAsync(
      queue: _settings.TransactionImportQueueName,
      durable: true,
      exclusive: false,
      autoDelete: false,
      arguments: null,
      cancellationToken: cancellationToken
    );

    return channel;
  }

  public async ValueTask DisposeAsync()
  {
    if (_connection is not null)
    {
      await _connection.DisposeAsync();
    }

    _connectionLock.Dispose();
  }

  private async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
  {
    if (_connection is { IsOpen: true })
    {
      return _connection;
    }

    await _connectionLock.WaitAsync(cancellationToken);

    try
    {
      if (_connection is { IsOpen: true })
      {
        return _connection;
      }

      if (_connection is not null)
      {
        await _connection.DisposeAsync();
      }

      var factory = new ConnectionFactory
      {
        HostName = _settings.Host,
        Port = _settings.Port,
        UserName = _settings.UserName,
        Password = _settings.Password,
        AutomaticRecoveryEnabled = true
      };

      _connection = await factory.CreateConnectionAsync(cancellationToken);

      return _connection;
    }
    finally
    {
      _connectionLock.Release();
    }
  }
}
