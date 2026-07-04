using api.Requests.Transaction;
using api.Models.FileProcessing;
using api.Models.Job;
using api.Services.File;
using api.Services.FileProcessing;
using api.Services.Job;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Transaction;

[ApiController]
[Tags("Transaction")]
[Route("/transaction/upload")]
public class UploadTransactionController(
    ILogger<UpdateTransactionController> logger,
    FileService fileService,
    FileProcessingService fileProcessingService,
    JobService jobService
) : ControllerBase
{
    private readonly ILogger<UpdateTransactionController> _logger = logger;
    private readonly FileService _fileService = fileService;
    private readonly FileProcessingService _fileProcessingService = fileProcessingService;
    private readonly JobService _jobService = jobService;

    [HttpPost]
    public async Task<ActionResult<FileProcessingModel>> ExecuteAsync([FromForm] UpladTransactionRequest request)
    {
        try
        {
            var file = await _fileService.CreateFromFormFileAsync(request.File);
            var jobId = Guid.NewGuid();
            var fileProcessing = await _fileProcessingService.CreateSubmitted(file.Id, jobId);

            await _jobService.QueueTransactionImport(new TransactionImportJobPayload
            {
                JobId = jobId,
                FileId = file.Id,
                FileProcessingId = fileProcessing.Id,
                CreatedAt = DateTime.UtcNow
            });

            return StatusCode(201, fileProcessing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UpdateTransactionController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
