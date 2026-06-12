using api.Requests;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Tags("Transaction")]
[Route("/transaction/upload")]
public class UploadTransactionController(
    ILogger<UpdateTransactionController> logger,
    UploadTransactionService service
) : ControllerBase
{
    private readonly ILogger<UpdateTransactionController> _logger = logger;
    private readonly UploadTransactionService _service = service;

    [HttpPost]
    public async Task<ActionResult> ExecuteAsync([FromForm] UpladTransactionRequest request)
    {
        try
        {
            await _service.ExecuteAsync(request);
            
            return StatusCode(200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UpdateTransactionController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}