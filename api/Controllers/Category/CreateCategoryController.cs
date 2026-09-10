using api.Exceptions;
using api.Requests.Category;
using api.Services.Category;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Category;

[ApiController]
[Tags("Category")]
[Route("/category")]
public class CreateCategoryController(
  ILogger<CreateCategoryController> logger,
  CategoryService service
) : ControllerBase
{
  [HttpPost]
  public async Task<ActionResult> ExecuteAsync([FromBody] CreateCategoryRequest request)
  {
    try
    {
      return StatusCode(201, await service.Create(request));
    }
    catch (ExistsCategoryException ex)
    {
      return StatusCode(409, new { Error = ex.Message });
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "[CreateCategoryController]");
      return StatusCode(500, new { error = "Internal server error" });
    }
  }
}
