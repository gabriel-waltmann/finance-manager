using api.Requests;
using api.Models;

namespace api.Services;

public class CreateTransactionService(DatabaseContext context)
{
  private readonly DatabaseContext _context = context; 

  private static Transaction Map(CreateTransactionRequest dto)
  {
    var id = Guid.NewGuid();

    return new Transaction
    {
      Id = id,
      Title = dto.Title,
      Date = dto.Date,
      Amount = dto.Amount,
      Created_at = DateTime.UtcNow
    };
  }

  public async Task<Transaction> ExecuteAsync(CreateTransactionRequest dto) {
    var transaction = Map(dto);

    _context.Transactions.Add(transaction);

    await _context.SaveChangesAsync();

    return transaction;
  }
}