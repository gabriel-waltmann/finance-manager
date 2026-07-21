using api.Requests.Transaction;
using api.Responses.Transaction;
using api.Services.FileProcessing;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Transaction;

[ApiController]
[Tags("Transaction Import")]
[Route("/transaction-imports")]
public class ListTransactionImportController(
  ILogger<ListTransactionImportController> logger,
  FileProcessingService service
) : ControllerBase
{
  private readonly ILogger<ListTransactionImportController> _logger = logger;
  private readonly FileProcessingService _service = service;

  [HttpGet]
  public async Task<ActionResult<ListTransactionImportResponse>> ExecuteAsync(
    [FromQuery] ListTransactionImportRequest request
  )
  {
    try
    {
      request.Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
      request.Status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim();
      request.Order = request.Order.Trim().ToLowerInvariant();

      return Ok(await _service.List(request));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "[ListTransactionImportController]");
      return StatusCode(500, new { error = "Internal server error" });
    }
  }
}
