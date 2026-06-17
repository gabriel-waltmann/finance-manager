using api.Requests.Transaction;
using api.Services.Azure.BlobStorage;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Transaction;

[ApiController]
[Tags("Transaction")]
[Route("/transaction/upload")]
public class UploadTransactionController(
    ILogger<UpdateTransactionController> logger,
    BlobStorageAzureService blobStorageAzureService
) : ControllerBase
{
    private readonly ILogger<UpdateTransactionController> _logger = logger;
    private readonly BlobStorageAzureService _blobStorageAzureService = blobStorageAzureService;

    [HttpPost]
    public async Task<ActionResult> ExecuteAsync([FromForm] UpladTransactionRequest request)
    {
        try
        {
            await _blobStorageAzureService.UploadBlobAsync(
                "files", 
                request.File.Name, 
                request.File.ContentType,
                request.File.OpenReadStream()
            );
            


            return StatusCode(200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UpdateTransactionController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}