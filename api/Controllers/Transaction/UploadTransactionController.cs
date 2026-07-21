using api.Requests.Transaction;
using api.Models.Job;
using api.Responses.Transaction;
using api.Services.File;
using api.Services.FileProcessing;
using api.Services.Job;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Transaction;

[ApiController]
[Tags("Transaction")]
[Route("/transaction/upload")]
public class UploadTransactionController(
    ILogger<UploadTransactionController> logger,
    FileService fileService,
    FileProcessingService fileProcessingService,
    JobService jobService
) : ControllerBase
{
    private readonly ILogger<UploadTransactionController> _logger = logger;
    private readonly FileService _fileService = fileService;
    private readonly FileProcessingService _fileProcessingService = fileProcessingService;
    private readonly JobService _jobService = jobService;

    [HttpPost]
    public async Task<ActionResult<TransactionImportResponse>> ExecuteAsync([FromForm] UpladTransactionRequest request)
    {
        try
        {
            if (request.Category is null)
            {
                return BadRequest(new { error = "Category is required." });
            }

            if (!Enum.IsDefined(request.Category.Value))
            {
                return BadRequest(new { error = "Category must be CreditCard or Extrato." });
            }

            var file = await _fileService.CreateFromFormFileAsync(request.File, request.Category.Value);
            var jobId = Guid.NewGuid();
            var fileProcessing = await _fileProcessingService.CreateSubmitted(file.Id, jobId);
            var response = await _fileProcessingService.GetResponse(fileProcessing.Id);

            try
            {
                await _jobService.QueueTransactionImport(new TransactionImportJobPayload
                {
                    JobId = jobId,
                    FileId = file.Id,
                    FileProcessingId = fileProcessing.Id,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch
            {
                await _fileProcessingService.MarkFailed(fileProcessing.Id);
                throw;
            }

            return StatusCode(201, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UploadTransactionController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
