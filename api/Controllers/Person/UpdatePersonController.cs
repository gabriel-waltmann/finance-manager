using api.Exceptions;
using api.Requests.Person;
using api.Services.Person;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Person;

[ApiController]
[Tags("Person")]
[Route("/person/{id}")]
public class UpdatePersonController(
    ILogger<UpdatePersonController> logger,
    PersonService service
) : ControllerBase
{
    private readonly ILogger<UpdatePersonController> _logger = logger;
    private readonly PersonService _service = service;

    [HttpPut]
    public async Task<ActionResult> ExecuteAsync([FromRoute] string id, [FromBody] UpdatePersonRequest request)
    {
        try
        {
            await _service.Update(Guid.Parse(id), request);

            return StatusCode(200);
        }
        catch (NotFoundPersonException ex)
        {
            return StatusCode(404, new { Error = ex.Message });
        }
        catch (ExistsPersonException ex)
        {
            return StatusCode(409, new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UpdatePersonController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
