using api.Exceptions;
using api.Requests.Common;
using api.Responses.Transaction;
using api.Services.Transaction;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Transaction;

[ApiController]
[Tags("Transaction")]
[Route("/transaction/{id}")]
public class GetTransactionController(
    ILogger<GetTransactionController> logger,
    TransactionService service
) : ControllerBase
{
    private readonly ILogger<GetTransactionController> _logger = logger;
    private readonly TransactionService _service = service;

    [HttpGet]
    public async Task<ActionResult<GetTransactionResponse>> ExecuteAsync(
        [FromRoute] RouteIdRequest route
    )
    {
        try
        {
            var transaction = await _service.GetWithTransactionPerson(route.Id);
            
            return StatusCode(200, transaction);
        }
        catch (NotFoundTransactionException ex)
        {
            return StatusCode(404, new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[GetTransactionController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
