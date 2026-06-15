using api.Requests.Transaction;
using api.Responses.Transaction;
using api.Services.Transaction;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Transaction;

[ApiController]
[Tags("Transaction")]
[Route("/transaction")]
public class CreateTransactionController(
    ILogger<CreateTransactionController> logger,
    CreateTransactionService service
) : ControllerBase
{
    private readonly ILogger<CreateTransactionController> _logger = logger;
    private readonly CreateTransactionService _service = service;
    
    [HttpPost]
    public async Task<ActionResult<ListTransactionResponse>> ExecuteAsync([FromBody] CreateTransactionRequest request)
    {
        try
        {
            var transaction = await _service.ExecuteAsync(request);

            return StatusCode(201, transaction);
        } catch (Exception ex)
        {
            _logger.LogError(ex, "[CreateTransactionController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
