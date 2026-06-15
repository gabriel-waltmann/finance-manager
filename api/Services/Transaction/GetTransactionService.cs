using api.Exceptions;
using api.Models.Database;
using api.Models.Transaction;

namespace api.Services.Transaction;

public class GetTransactionService(DatabaseContext context)
{
  private readonly DatabaseContext _context = context; 

  public async Task<TransactionModel> ExecuteAsync(Guid id)
  {
    return await _context.Transactions.FindAsync(id) ?? throw new NotFoundTransactionException();
  }
}