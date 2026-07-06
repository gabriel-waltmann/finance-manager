using api.Models.TransactionPerson;
using api.Responses.TransactionPerson;
using api.Services.TransactionPerson;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.TransactionPerson;

[ApiController]
[Tags("TransactionPerson")]
[Route("/transaction-persons")]
public class ListTransactionPersonController(
    ILogger<ListTransactionPersonController> logger,
    TransactionPersonService service
) : ControllerBase
{
    private readonly ILogger<ListTransactionPersonController> _logger = logger;
    private readonly TransactionPersonService _service = service;

    public static ListTransactionPersonResponse MapResponse(List<TransactionPersonModel> transactionPersons)
    {
        return new ListTransactionPersonResponse
        {
            TransactionPersons = transactionPersons
        };
    }

    [HttpGet]
    public async Task<ActionResult<ListTransactionPersonResponse>> ExecuteAsync([FromQuery] string? withDeleted)
    {
        try
        {
            var transactionPersons = await _service.List(withDeleted == "true");

            var response = MapResponse(transactionPersons);

            return StatusCode(200, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ListTransactionPersonController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
