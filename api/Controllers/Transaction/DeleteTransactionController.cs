using api.Exceptions;
using api.Services.Transaction;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Transaction;

[ApiController]
[Tags("Transaction")]
[Route("/transaction/{id}")]
public class DeleteTransactionController(
    ILogger<DeleteTransactionController> logger,
    TransactionService service
) : ControllerBase
{
    private readonly ILogger<DeleteTransactionController> _logger = logger;
    private readonly TransactionService _service = service;

    [HttpDelete]
    public async Task<ActionResult> ExecuteAsync([FromRoute] string id)
    {
        try
        {
            await _service.Delete(Guid.Parse(id));
            
            return StatusCode(200);
        }
        catch (NotFoundTransactionException)
        {
            return StatusCode(200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DeleteTransactionController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
