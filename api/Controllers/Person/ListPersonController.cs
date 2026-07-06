using api.Models.Person;
using api.Responses.Person;
using api.Services.Person;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Person;

[ApiController]
[Tags("Person")]
[Route("/persons")]
public class ListPersonController(
    ILogger<ListPersonController> logger,
    PersonService service
) : ControllerBase
{
    private readonly ILogger<ListPersonController> _logger = logger;
    private readonly PersonService _service = service;

    public static ListPersonResponse MapResponse(List<PersonModel> persons)
    {
        return new ListPersonResponse
        {
            Persons = persons
        };
    }

    [HttpGet]
    public async Task<ActionResult<ListPersonResponse>> ExecuteAsync([FromQuery] string? withDeleted)
    {
        try
        {
            var persons = await _service.List(withDeleted == "true");

            var response = MapResponse(persons);

            return StatusCode(200, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ListPersonController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
