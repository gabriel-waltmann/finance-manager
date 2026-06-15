using api.Exceptions;
using api.Models.Transaction;
using api.Services.Transaction;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Transaction;

[ApiController]
[Tags("Transaction")]
[Route("/transaction/{id}")]
public class GetTransactionController(
    ILogger<GetTransactionController> logger,
    GetTransactionService service
) : ControllerBase
{
    private readonly ILogger<GetTransactionController> _logger = logger;
    private readonly GetTransactionService _service = service;

    [HttpGet]
    public async Task<ActionResult<TransactionModel>> ExecuteAsync([FromRoute] string id)
    {
        try
        {
            var transaction = await _service.ExecuteAsync(Guid.Parse(id));
            
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
