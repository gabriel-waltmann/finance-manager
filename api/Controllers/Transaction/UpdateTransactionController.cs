using api.Requests.Transaction;
using api.Services.Transaction;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Transaction;

[ApiController]
[Tags("Transaction")]
[Route("/transaction/{id}")]
public class UpdateTransactionController(
    ILogger<UpdateTransactionController> logger,
    TransactionService service
) : ControllerBase
{
    private readonly ILogger<UpdateTransactionController> _logger = logger;
    private readonly TransactionService _service = service;

    [HttpPut]
    public async Task<ActionResult> ExecuteAsync([FromRoute] string id, [FromBody] UpdateTransactionRequest dto)
    {
        try
        {
            await _service.Update(Guid.Parse(id), dto);
            
            return StatusCode(200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UpdateTransactionController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
