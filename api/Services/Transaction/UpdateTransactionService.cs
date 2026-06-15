using api.Requests.Transaction;
using api.Exceptions;
using api.Models.Database;

namespace api.Services.Transaction;

public class UpdateTransactionService(DatabaseContext context)
{
  private readonly DatabaseContext _context = context;
  
  public async Task ExecuteAsync(Guid id, UpdateTransactionRequest request)
  {
    var transaction = await _context.Transactions.FindAsync(id) ?? throw new NotFoundTransactionException();

    transaction.Title = request.Title;
    transaction.Date = request.Date;
    transaction.Amount = request.Amount;
    transaction.Updated_at = DateTime.UtcNow;
    
    await _context.SaveChangesAsync();
  }
}