using api.Exceptions;
using api.Models.Person;
using api.Services.Person;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Person;

[ApiController]
[Tags("Person")]
[Route("/person/{id}")]
public class GetPersonController(
    ILogger<GetPersonController> logger,
    PersonService service
) : ControllerBase
{
    private readonly ILogger<GetPersonController> _logger = logger;
    private readonly PersonService _service = service;

    [HttpGet]
    public async Task<ActionResult<PersonModel>> ExecuteAsync([FromRoute] string id)
    {
        try
        {
            var person = await _service.Get(Guid.Parse(id));

            return StatusCode(200, person);
        }
        catch (NotFoundPersonException ex)
        {
            return StatusCode(404, new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetPersonController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
