using api.Requests;
using api.Models;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Tags("Transaction")]
[Route("/transaction/{id}")]
public class UpdateTransactionController(
    ILogger<UpdateTransactionController> logger,
    UpdateTransactionService service
) : ControllerBase
{
    private readonly ILogger<UpdateTransactionController> _logger = logger;
    private readonly UpdateTransactionService _service = service;

    [HttpPut]
    public async Task<ActionResult> ExecuteAsync([FromRoute] string id, [FromBody] UpdateTransactionRequest dto)
    {
        try
        {
            await _service.ExecuteAsync(Guid.Parse(id), dto);
            
            return StatusCode(200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UpdateTransactionController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
