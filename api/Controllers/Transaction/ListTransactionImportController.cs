using api.Models.FileProcessingStatus;
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
      if (request.Page < 1)
      {
        return BadRequest(new { error = "Page must be greater than or equal to 1." });
      }

      if (request.Limit < 1 || request.Limit > 100)
      {
        return BadRequest(new { error = "Limit must be between 1 and 100." });
      }

      request.Search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
      request.Status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim();
      request.Order = request.Order.Trim().ToLowerInvariant();

      if (request.Order != "asc" && request.Order != "desc")
      {
        return BadRequest(new { error = "Order must be asc or desc." });
      }

      if (
        request.Status is not null &&
        !Enum.TryParse<FileProcessingStatusName>(request.Status, ignoreCase: true, out _)
      )
      {
        return BadRequest(new { error = "Status must be Submitted, Processing, Finished, or Failed." });
      }

      return Ok(await _service.List(request));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "[ListTransactionImportController]");
      return StatusCode(500, new { error = "Internal server error" });
    }
  }
}
