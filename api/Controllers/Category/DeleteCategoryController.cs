using api.Requests.Common;
using api.Services.Category;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Category;

[ApiController]
[Tags("Category")]
[Route("/category/{id}")]
public class DeleteCategoryController(
  ILogger<DeleteCategoryController> logger,
  CategoryService service
) : ControllerBase
{
  [HttpDelete]
  public async Task<ActionResult> ExecuteAsync([FromRoute] RouteIdRequest route)
  {
    try
    {
      await service.Delete(route.Id);
      return StatusCode(200);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "[DeleteCategoryController]");
      return StatusCode(500, new { error = "Internal server error" });
    }
  }
}
