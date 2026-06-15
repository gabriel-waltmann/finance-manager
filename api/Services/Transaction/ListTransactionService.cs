using api.Models.Database;
using api.Models.Transaction;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Transaction;

public class ListTransactionService(DatabaseContext context)
{
  private readonly DatabaseContext _context = context; 

  public async Task<List<TransactionModel>> ExecuteAsync()
  {
    return await _context.Transactions
      .Where(transaction => transaction.Deleted_at == null)
      .ToListAsync();
  }
}