using api.Models.Database;
using api.Models.TransactionImport;

namespace api.Services.TransactionImport;

public class TransactionImportService(DatabaseContext context)
{
  private readonly DatabaseContext _context = context;

  public async Task<TransactionImportModel> Create(
    Guid fileProcessingId,
    Guid transactionId,
    CancellationToken cancellationToken = default
  )
  {
    var model = new TransactionImportModel
    {
      Id = Guid.NewGuid(),
      FileProcessingId = fileProcessingId,
      TransactionId = transactionId,
      Created_at = DateTime.UtcNow
    };

    _context.TransactionsImport.Add(model);

    await _context.SaveChangesAsync(cancellationToken);

    return model;
  }
}
