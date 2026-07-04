using System.Text.Json;
using api.Models.Job;
using StackExchange.Redis;

namespace api.Services.Job;

public class JobService(IConnectionMultiplexer connectionMultiplexer)
{
  public const string TransactionImportQueueKey = "jobs:transaction-import";

  private readonly IDatabase _redis = connectionMultiplexer.GetDatabase();

  public async Task QueueTransactionImport(TransactionImportJobPayload payload)
  {
    var json = JsonSerializer.Serialize(payload);

    await _redis.ListRightPushAsync(TransactionImportQueueKey, json);
  }
}
