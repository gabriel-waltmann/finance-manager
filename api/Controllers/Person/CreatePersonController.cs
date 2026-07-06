using api.Exceptions;
using api.Requests.Person;
using api.Services.Person;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Person;

[ApiController]
[Tags("Person")]
[Route("/person")]
public class CreatePersonController(
    ILogger<CreatePersonController> logger,
    PersonService service
) : ControllerBase
{
    private readonly ILogger<CreatePersonController> _logger = logger;
    private readonly PersonService _service = service;

    [HttpPost]
    public async Task<ActionResult> ExecuteAsync([FromBody] CreatePersonRequest request)
    {
        try
        {
            var person = await _service.Create(request);

            return StatusCode(201, person);
        }
        catch (ExistsPersonException ex)
        {
            return StatusCode(409, new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CreatePersonController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
