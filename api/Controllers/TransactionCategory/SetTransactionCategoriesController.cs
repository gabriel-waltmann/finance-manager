using api.Exceptions;
using api.Requests.Common;
using api.Requests.TransactionCategory;
using api.Responses.TransactionCategory;
using api.Services.TransactionCategory;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.TransactionCategory;

[ApiController]
[Tags("TransactionCategory")]
[Route("/transaction/{id}/categories")]
public class SetTransactionCategoriesController(
  ILogger<SetTransactionCategoriesController> logger,
  TransactionCategoryService service
) : ControllerBase
{
  [HttpPut]
  public async Task<ActionResult<SetTransactionCategoriesResponse>> ExecuteAsync(
    [FromRoute] RouteIdRequest route,
    [FromBody] SetTransactionCategoriesRequest request
  )
  {
    try
    {
      return StatusCode(200, await service.Set(route.Id, request));
    }
    catch (NotFoundTransactionException ex)
    {
      return StatusCode(404, new { Error = ex.Message });
    }
    catch (NotFoundCategoryException ex)
    {
      return StatusCode(404, new { Error = ex.Message });
    }
    catch (ExistsTransactionCategoryException ex)
    {
      return StatusCode(409, new { Error = ex.Message });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "[SetTransactionCategoriesController]");
      return StatusCode(500, new { error = "Internal server error" });
    }
  }
}
