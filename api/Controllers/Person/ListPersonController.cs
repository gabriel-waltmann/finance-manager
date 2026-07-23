using api.Requests.Person;
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

    [HttpGet]
    public async Task<ActionResult<ListPersonResponse>> ExecuteAsync(
        [FromQuery] ListPersonRequest request
    )
    {
        try
        {
            request.Search = string.IsNullOrWhiteSpace(request.Search)
                ? null
                : request.Search.Trim();
            request.Order = request.Order.Trim().ToLowerInvariant();

            var response = await _service.List(request);

            return StatusCode(200, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ListPersonController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
