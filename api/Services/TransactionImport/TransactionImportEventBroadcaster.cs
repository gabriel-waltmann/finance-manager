using System.Collections.Concurrent;
using System.Threading.Channels;
using api.Responses.Transaction;

namespace api.Services.TransactionImport;

public class TransactionImportEventBroadcaster
{
  private readonly ConcurrentDictionary<Guid, Channel<TransactionImportResponse>> _subscribers = new();

  public TransactionImportEventSubscription Subscribe()
  {
    var id = Guid.NewGuid();
    var channel = Channel.CreateBounded<TransactionImportResponse>(new BoundedChannelOptions(20)
    {
      FullMode = BoundedChannelFullMode.DropOldest,
      SingleReader = true,
      SingleWriter = false
    });

    _subscribers[id] = channel;

    return new TransactionImportEventSubscription(id, channel.Reader);
  }

  public void Unsubscribe(Guid id)
  {
    if (_subscribers.TryRemove(id, out var channel))
    {
      channel.Writer.TryComplete();
    }
  }

  public void Publish(TransactionImportResponse response)
  {
    foreach (var channel in _subscribers.Values)
    {
      channel.Writer.TryWrite(response);
    }
  }
}

public record TransactionImportEventSubscription(
  Guid Id,
  ChannelReader<TransactionImportResponse> Reader
);
