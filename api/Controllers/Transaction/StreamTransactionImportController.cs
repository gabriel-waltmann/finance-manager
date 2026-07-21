using System.Text.Json;
using api.Services.TransactionImport;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Transaction;

[ApiController]
[Tags("Transaction Import")]
[Route("/transaction-imports/events")]
public class StreamTransactionImportController(
  TransactionImportEventBroadcaster eventBroadcaster
) : ControllerBase
{
  private readonly TransactionImportEventBroadcaster _eventBroadcaster = eventBroadcaster;
  private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

  [HttpGet]
  public async Task ExecuteAsync(CancellationToken cancellationToken)
  {
    Response.StatusCode = StatusCodes.Status200OK;
    Response.ContentType = "text/event-stream";
    Response.Headers.CacheControl = "no-cache, no-store";
    Response.Headers.Append("X-Accel-Buffering", "no");
    HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

    var subscription = _eventBroadcaster.Subscribe();

    try
    {
      await Response.WriteAsync("retry: 3000\n: connected\n\n", cancellationToken);
      await Response.Body.FlushAsync(cancellationToken);

      while (!cancellationToken.IsCancellationRequested)
      {
        using var heartbeatCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        heartbeatCancellation.CancelAfter(TimeSpan.FromSeconds(15));

        try
        {
          if (await subscription.Reader.WaitToReadAsync(heartbeatCancellation.Token))
          {
            while (subscription.Reader.TryRead(out var import))
            {
              var json = JsonSerializer.Serialize(import, JsonOptions);
              await Response.WriteAsync("event: transaction-import-status\n", cancellationToken);
              await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
            }
          }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
          await Response.WriteAsync(": heartbeat\n\n", cancellationToken);
        }

        await Response.Body.FlushAsync(cancellationToken);
      }
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      // The browser closed the stream.
    }
    finally
    {
      _eventBroadcaster.Unsubscribe(subscription.Id);
    }
  }
}
