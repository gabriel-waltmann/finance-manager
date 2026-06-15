using api.Helpers.File.Csv;
using api.Mappers.Transaction;
using api.Models.Files;
using api.Models.Transaction;
using api.Requests.Transaction;
using api.Services.Transaction;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Transaction;

[ApiController]
[Tags("Transaction")]
[Route("/transaction/upload")]
public class UploadTransactionController(
    ILogger<UpdateTransactionController> logger,
    ListTransactionService listService,
    CreateTransactionService createService,
    UploadTransactionMapper mapper
) : ControllerBase
{
    private readonly ILogger<UpdateTransactionController> _logger = logger;
    private readonly ListTransactionService _listService = listService;
    private readonly CreateTransactionService _createService = createService;

    private static bool Exists(
        List<TransactionModel> transactions, 
        CreateTransactionRequest request
    )
    {
        return transactions.Exists(transaction => 
            request.Title == transaction.Title &&
            request.Date == transaction.Date &&
            request.Amount == transaction.Amount && 
            transaction.Deleted_at == null
        );
    }
    
    [HttpPost]
    public async Task<ActionResult> ExecuteAsync([FromForm] UpladTransactionRequest request)
    {
        try
        {
            var transactions = await _listService.ExecuteAsync(true);

            var fileItems = CsvFileHelper.Convert<CreditCardNubankFile>(request.File);

            foreach (var fileItem in fileItems)
            {
                var createRequest = mapper.MapCreateRequest(fileItem);

                if (createRequest == null) continue;

                if (Exists(transactions, createRequest)) continue;

                await _createService.ExecuteAsync(createRequest);
            }
            
            return StatusCode(200);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UpdateTransactionController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}