using api.Requests.Transaction;
using api.Responses.Transaction;
using api.Services.Transaction;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Transaction;

[ApiController]
[Tags("Transaction")]
[Route("/transactions")]
public class ListTransactionController(
    ILogger<ListTransactionController> logger,
    TransactionService service
) : ControllerBase
{
    private readonly ILogger<ListTransactionController> _logger = logger;
    private readonly TransactionService _service = service;

  
    [HttpGet]
    public async Task<ActionResult<ListTransactionResponse>> ExecuteAsync(
        [FromQuery] ListTransactionRequest request
    )
    {
        try
        {
            request.Search = string.IsNullOrWhiteSpace(request.Search)
                ? null
                : request.Search.Trim();
            request.Order = request.Order.Trim().ToLowerInvariant();

            var response = await _service.ListWithTransactionPerson(request);
            
            return StatusCode(200, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ListTransactionController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
