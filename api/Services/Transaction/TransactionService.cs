using api.Requests.Transaction;
using api.Models.Transaction;
using api.Models.Database;
using api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Transaction;

public class TransactionService(DatabaseContext context)
{
  private readonly DatabaseContext _context = context; 

  private static TransactionModel MapCreate(CreateTransactionRequest dto)
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

  public async Task<TransactionModel> Get(Guid id)
  {
    return await _context.Transactions.FirstOrDefaultAsync(transaction =>
      transaction.Id == id &&
      transaction.Deleted_at == null
    ) ?? throw new NotFoundTransactionException();
  }

  public async Task<List<TransactionModel>> List(bool withDeleted)
  {
    return await _context.Transactions
      .Where(transaction => withDeleted || transaction.Deleted_at == null)
      .ToListAsync();
  }

  public async Task<TransactionModel> Create(CreateTransactionRequest dto) {
    var exists = await _context.Transactions.AnyAsync(transaction =>
      transaction.Deleted_at == null &&
      transaction.Date == dto.Date &&
      transaction.Title == dto.Title &&
      transaction.Amount == dto.Amount
    );

    if (exists)
    {
      throw new ExistsTransactionException();
    }

    var transaction = MapCreate(dto);

    _context.Transactions.Add(transaction);

    await _context.SaveChangesAsync();

    return transaction;
  }

  public async Task Update(Guid id, UpdateTransactionRequest request)
  {
    var transaction = await _context.Transactions.FindAsync(id) ?? throw new NotFoundTransactionException();

    transaction.Title = request.Title;
    transaction.Date = request.Date;
    transaction.Amount = request.Amount;
    transaction.Updated_at = DateTime.UtcNow;
    
    await _context.SaveChangesAsync();
  }
  
  public async Task Delete(Guid id)
  {
    var transaction = await _context.Transactions.FindAsync(id) ?? throw new NotFoundTransactionException();

    transaction.Deleted_at = DateTime.UtcNow;

    await _context.SaveChangesAsync();
  }
}
