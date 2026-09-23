using api.Exceptions;
using api.Requests.Transaction;
using api.Responses.Transaction;
using api.Services.Transaction;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Transaction;

[ApiController]
[Tags("Transaction")]
[Route("/transactions/auto-assign")]
public class AutoAssignTransactionsController(
  ILogger<AutoAssignTransactionsController> logger,
  TransactionService service
) : ControllerBase
{
  [HttpPost]
  public async Task<ActionResult<AutoAssignTransactionsResponse>> ExecuteAsync(
    [FromBody] AutoAssignTransactionsRequest request
  )
  {
    try
    {
      return StatusCode(200, await service.AutoAssign(request));
    }
    catch (NotFoundPersonException ex)
    {
      return StatusCode(404, new { Error = ex.Message });
    }
    catch (NotFoundCategoryException ex)
    {
      return StatusCode(404, new { Error = ex.Message });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "[AutoAssignTransactionsController]");
      return StatusCode(500, new { error = "Internal server error" });
    }
  }
}
