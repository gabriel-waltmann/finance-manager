using System.Transactions;
using api.Requests;
using api.Mappers;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Tags("Transaction")]
[Route("/transactions")]
public class ListTransactionController(
    ILogger<ListTransactionController> logger,
    ListTransactionMapper mapper,
    ListTransactionService service
) : ControllerBase
{
    private readonly ILogger<ListTransactionController> _logger = logger;
    private readonly ListTransactionService _service = service;
    private readonly ListTransactionMapper _mapper = mapper;

    [HttpGet]
    public async Task<ActionResult<List<Transaction>>> ExecuteAsync()
    {
        try
        {
            var transactions = await _service.ExecuteAsync();

            var response = _mapper.MapResponse(transactions);
            
            return StatusCode(200, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ListTransactionController]");

            return StatusCode(500, new { error = "Internal server error"} );
        }
    }
}
