using api.Exceptions;
using api.Models.Category;
using api.Requests.Common;
using api.Services.Category;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Category;

[ApiController]
[Tags("Category")]
[Route("/category/{id}")]
public class GetCategoryController(
  ILogger<GetCategoryController> logger,
  CategoryService service
) : ControllerBase
{
  [HttpGet]
  public async Task<ActionResult<CategoryModel>> ExecuteAsync([FromRoute] RouteIdRequest route)
  {
    try
    {
      return StatusCode(200, await service.Get(route.Id));
    }
    catch (NotFoundCategoryException ex)
    {
      return StatusCode(404, new { Error = ex.Message });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "[GetCategoryController]");
      return StatusCode(500, new { error = "Internal server error" });
    }
  }
}
