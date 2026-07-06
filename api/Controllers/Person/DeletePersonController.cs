using api.Exceptions;
using api.Services.Person;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Person;

[ApiController]
[Tags("Person")]
[Route("/person/{id}")]
public class DeletePersonController(
    ILogger<DeletePersonController> logger,
    PersonService service
) : ControllerBase
{
    private readonly ILogger<DeletePersonController> _logger = logger;
    private readonly PersonService _service = service;

    [HttpDelete]
    public async Task<ActionResult> ExecuteAsync([FromRoute] string id)
    {
        try
        {
            await _service.Delete(Guid.Parse(id));

            return StatusCode(200);
        }
        catch (NotFoundPersonException)
        {
            return StatusCode(200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DeletePersonController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
