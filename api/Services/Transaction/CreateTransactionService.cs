using api.Requests.Transaction;
using api.Models.Transaction;
using api.Models.Database;

namespace api.Services.Transaction;

public class CreateTransactionService(DatabaseContext context)
{
  private readonly DatabaseContext _context = context; 

  private static TransactionModel Map(CreateTransactionRequest dto)
  {
    var id = Guid.NewGuid();

    return new TransactionModel
    {
      Id = id,
      Title = dto.Title,
      Date = dto.Date,
      Amount = dto.Amount,
      Created_at = DateTime.UtcNow
    };
  }

  public async Task<TransactionModel> ExecuteAsync(CreateTransactionRequest dto) {
    var transaction = Map(dto);

    _context.Transactions.Add(transaction);

    await _context.SaveChangesAsync();

    return transaction;
  }
}