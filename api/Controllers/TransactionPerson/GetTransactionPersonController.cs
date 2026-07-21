using api.Exceptions;
using api.Models.TransactionPerson;
using api.Requests.Common;
using api.Services.TransactionPerson;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.TransactionPerson;

[ApiController]
[Tags("TransactionPerson")]
[Route("/transaction-person/{id}")]
public class GetTransactionPersonController(
    ILogger<GetTransactionPersonController> logger,
    TransactionPersonService service
) : ControllerBase
{
    private readonly ILogger<GetTransactionPersonController> _logger = logger;
    private readonly TransactionPersonService _service = service;

    [HttpGet]
    public async Task<ActionResult<TransactionPersonModel>> ExecuteAsync(
        [FromRoute] RouteIdRequest route
    )
    {
        try
        {
            var transactionPerson = await _service.Get(route.Id);

            return StatusCode(200, transactionPerson);
        }
        catch (NotFoundTransactionPersonException ex)
        {
            return StatusCode(404, new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetTransactionPersonController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
