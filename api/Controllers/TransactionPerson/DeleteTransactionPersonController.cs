using api.Exceptions;
using api.Requests.Common;
using api.Services.TransactionPerson;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.TransactionPerson;

[ApiController]
[Tags("TransactionPerson")]
[Route("/transaction-person/{id}")]
public class DeleteTransactionPersonController(
    ILogger<DeleteTransactionPersonController> logger,
    TransactionPersonService service
) : ControllerBase
{
    private readonly ILogger<DeleteTransactionPersonController> _logger = logger;
    private readonly TransactionPersonService _service = service;

    [HttpDelete]
    public async Task<ActionResult> ExecuteAsync([FromRoute] RouteIdRequest route)
    {
        try
        {
            await _service.Delete(route.Id);

            return StatusCode(200);
        }
        catch (NotFoundTransactionPersonException)
        {
            return StatusCode(200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DeleteTransactionPersonController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
