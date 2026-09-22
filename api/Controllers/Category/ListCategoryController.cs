using api.Requests.Category;
using api.Responses.Category;
using api.Services.Category;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Category;

[ApiController]
[Tags("Category")]
[Route("/categories")]
public class ListCategoryController(
  ILogger<ListCategoryController> logger,
  CategoryService service
) : ControllerBase
{
  [HttpGet]
  public async Task<ActionResult<ListCategoryResponse>> ExecuteAsync(
    [FromQuery] ListCategoryRequest request
  )
  {
    try
    {
      return StatusCode(200, await service.List(request));
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "[ListCategoryController]");
      return StatusCode(500, new { error = "Internal server error" });
    }
  }
}
