using api.Requests.Transaction;
using api.Responses.Transaction;
using api.Services.Transaction;
using api.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Transaction;

[ApiController]
[Tags("Transaction")]
[Route("/transaction")]
public class CreateTransactionController(
    ILogger<CreateTransactionController> logger,
    TransactionService service
) : ControllerBase
{
    private readonly ILogger<CreateTransactionController> _logger = logger;
    private readonly TransactionService _service = service;
    
    [HttpPost]
    public async Task<ActionResult<ListTransactionResponse>> ExecuteAsync([FromBody] CreateTransactionRequest request)
    {
        try
        {
            var transaction = await _service.Create(request);

            return StatusCode(201, transaction);
        }
        catch (ExistsTransactionException ex)
        {
            return StatusCode(409, new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CreateTransactionController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
