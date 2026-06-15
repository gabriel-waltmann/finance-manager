using System.Globalization;
using api.Helpers.File.Csv;
using api.Mappers;
using api.Models;
using api.Models.Files;
using api.Requests;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

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

    private static Transaction Exists(List<Transaction> transactions, CreateTransactionRequest request)
    {
        foreach (var transaction in transactions)
        {
            var someTitle = request.Title == transaction.Title;
            var someDate = request.Date == transaction.Date;
            var someAmont = request.Amount == transaction.Amount;

            return someTitle && someDate && someAmont ? transaction : null;
        }

        return null;
    }
    
    [HttpPost]
    public async Task<ActionResult> ExecuteAsync([FromForm] UpladTransactionRequest request)
    {
        try
        {
            var transactions = await _listService.ExecuteAsync();

            var fileItems = CsvFileHelper.Convert<CreditCardNubankFile>(request.File);

            foreach (var fileItem in fileItems)
            {
                var createRequest = mapper.MapCreateRequest(fileItem);

                if (createRequest == null) continue;

                var exist = Exists(transactions, createRequest);

                if (exist == null) continue;

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