using api.Exceptions;
using api.Requests.Category;
using api.Requests.Common;
using api.Services.Category;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Category;

[ApiController]
[Tags("Category")]
[Route("/category/{id}")]
public class UpdateCategoryController(
  ILogger<UpdateCategoryController> logger,
  CategoryService service
) : ControllerBase
{
  [HttpPut]
  public async Task<ActionResult> ExecuteAsync(
    [FromRoute] RouteIdRequest route,
    [FromBody] UpdateCategoryRequest request
  )
  {
    try
    {
      await service.Update(route.Id, request);
      return StatusCode(200);
    }
    catch (NotFoundCategoryException ex)
    {
      return StatusCode(404, new { Error = ex.Message });
    }
    catch (ExistsCategoryException ex)
    {
      return StatusCode(409, new { Error = ex.Message });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "[UpdateCategoryController]");
      return StatusCode(500, new { error = "Internal server error" });
    }
  }
}
