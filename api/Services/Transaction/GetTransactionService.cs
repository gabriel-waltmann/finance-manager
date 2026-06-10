using api.Exceptions;
using api.Models;

namespace api.Services;

public class GetTransactionService(DatabaseContext context)
{
  private readonly DatabaseContext _context = context; 

  public async Task<Transaction> ExecuteAsync(Guid id)
  {
    return await _context.Transactions.FindAsync(id) ?? throw new NotFoundTransactionException();
  }
}