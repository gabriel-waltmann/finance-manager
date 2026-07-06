using api.Exceptions;
using api.Requests.TransactionPerson;
using api.Services.TransactionPerson;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.TransactionPerson;

[ApiController]
[Tags("TransactionPerson")]
[Route("/transaction-person")]
public class CreateTransactionPersonController(
    ILogger<CreateTransactionPersonController> logger,
    TransactionPersonService service
) : ControllerBase
{
    private readonly ILogger<CreateTransactionPersonController> _logger = logger;
    private readonly TransactionPersonService _service = service;

    [HttpPost]
    public async Task<ActionResult> ExecuteAsync([FromBody] CreateTransactionPersonRequest request)
    {
        try
        {
            var transactionPerson = await _service.Create(request);

            return StatusCode(201, transactionPerson);
        }
        catch (ExistsTransactionPersonException ex)
        {
            return StatusCode(409, new { Error = ex.Message });
        }
        catch (NotFoundPersonException ex)
        {
            return StatusCode(404, new { Error = ex.Message });
        }
        catch (NotFoundTransactionException ex)
        {
            return StatusCode(404, new { Error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[CreateTransactionPersonController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
