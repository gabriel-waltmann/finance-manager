using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class ListTransactionService(DatabaseContext context)
{
  private readonly DatabaseContext _context = context; 

  public async Task<List<Transaction>> ExecuteAsync()
  {
    return await _context.Transactions
      .Where(transaction => transaction.Deleted_at == null)
      .ToListAsync();
  }
}