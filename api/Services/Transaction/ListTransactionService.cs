using api.Models.Database;
using api.Models.Transaction;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Transaction;

public class ListTransactionService(DatabaseContext context)
{
  private readonly DatabaseContext _context = context; 

  public async Task<List<TransactionModel>> ExecuteAsync(bool withDeleted)
  {
    return await _context.Transactions
      .Where(transaction => withDeleted || transaction.Deleted_at == null)
      .ToListAsync();
  }
}