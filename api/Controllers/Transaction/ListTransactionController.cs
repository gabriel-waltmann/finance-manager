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
            if (request.Page < 1)
            {
                return BadRequest(new { error = "Page must be greater than or equal to 1." });
            }

            if (request.Limit < 1 || request.Limit > 100)
            {
                return BadRequest(new { error = "Limit must be between 1 and 100." });
            }

            request.Search = string.IsNullOrWhiteSpace(request.Search)
                ? null
                : request.Search.Trim();
            request.Order = request.Order.Trim().ToLowerInvariant();

            if (request.Order != "asc" && request.Order != "desc")
            {
                return BadRequest(new { error = "Order must be asc or desc." });
            }

            if (request.PersonId.HasValue && request.Unassigned)
            {
                return BadRequest(new { error = "PersonId and unassigned cannot be used together." });
            }

            if (
                request.StartDate.HasValue &&
                request.EndDate.HasValue &&
                request.StartDate.Value.Date > request.EndDate.Value.Date
            )
            {
                return BadRequest(new { error = "Start date must be before or equal to end date." });
            }

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
