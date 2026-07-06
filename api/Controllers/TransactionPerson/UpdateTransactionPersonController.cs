using api.Exceptions;
using api.Requests.TransactionPerson;
using api.Services.TransactionPerson;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.TransactionPerson;

[ApiController]
[Tags("TransactionPerson")]
[Route("/transaction-person/{id}")]
public class UpdateTransactionPersonController(
    ILogger<UpdateTransactionPersonController> logger,
    TransactionPersonService service
) : ControllerBase
{
    private readonly ILogger<UpdateTransactionPersonController> _logger = logger;
    private readonly TransactionPersonService _service = service;

    [HttpPut]
    public async Task<ActionResult> ExecuteAsync([FromRoute] string id, [FromBody] UpdateTransactionPersonRequest request)
    {
        try
        {
            await _service.Update(Guid.Parse(id), request);

            return StatusCode(200);
        }
        catch (ExistsTransactionPersonException ex)
        {
            return StatusCode(409, new { Error = ex.Message });
        }
        catch (NotFoundTransactionPersonException ex)
        {
            return StatusCode(404, new { Error = ex.Message });
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
            _logger.LogError(ex, "[UpdateTransactionPersonController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
