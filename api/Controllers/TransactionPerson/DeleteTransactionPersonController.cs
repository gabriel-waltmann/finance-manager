using api.Exceptions;
using api.Services.TransactionPerson;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.TransactionPerson;

[ApiController]
[Tags("TransactionPerson")]
[Route("/transaction-person/{id}")]
public class DeleteTransactionPersonController(
    ILogger<DeleteTransactionPersonController> logger,
    TransactionPersonService service
) : ControllerBase
{
    private readonly ILogger<DeleteTransactionPersonController> _logger = logger;
    private readonly TransactionPersonService _service = service;

    [HttpDelete]
    public async Task<ActionResult> ExecuteAsync([FromRoute] string id)
    {
        try
        {
            await _service.Delete(Guid.Parse(id));

            return StatusCode(200);
        }
        catch (NotFoundTransactionPersonException)
        {
            return StatusCode(200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DeleteTransactionPersonController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
