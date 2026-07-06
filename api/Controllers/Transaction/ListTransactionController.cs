using api.Services.Transaction;
using Microsoft.AspNetCore.Mvc;
using api.Responses.Transaction;

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

    public static ListTransactionResponse MapResponse(List<GetTransactionResponse> transactions)
    {
        return new ListTransactionResponse
        {
            Transactions = transactions
        };
    }
  
    [HttpGet]
    public async Task<ActionResult<ListTransactionResponse>> ExecuteAsync(
        [FromQuery] string? withDeleted
    )
    {
        try
        {
            var transactions = await _service.ListWithTransactionPerson(withDeleted == "true");

            var response = MapResponse(transactions);
            
            return StatusCode(200, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ListTransactionController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
