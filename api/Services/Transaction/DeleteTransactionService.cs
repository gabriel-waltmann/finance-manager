using api.Exceptions;
using api.Models;

namespace api.Services;

public class DeleteTransactionService(DatabaseContext context)
{
  private readonly DatabaseContext _context = context; 

  public async Task ExecuteAsync(Guid id)
  {
    var transaction = await _context.Transactions.FindAsync(id) ?? throw new NotFoundTransactionException();

    transaction.Deleted_at = DateTime.UtcNow;

    await _context.SaveChangesAsync();
  }
}