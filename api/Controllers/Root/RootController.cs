using api.Responses.Root;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Root;

[ApiController]
[Tags("Root")]
[Route("/")]
public class RootController : ControllerBase
{
  [HttpGet]
  public async Task<ActionResult<RootResponse>> ExecuteAsync()
  {
    var output = new RootResponse { Message = "Finance Manager API running.." };

    return StatusCode(200, output);
  }
}