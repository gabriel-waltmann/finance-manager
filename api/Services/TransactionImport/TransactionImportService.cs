using api.Exceptions;
using api.Models.Database;
using api.Models.TransactionCategory;
using api.Models.TransactionImport;
using api.Models.TransactionPerson;
using Microsoft.EntityFrameworkCore;

namespace api.Services.TransactionImport;

public class TransactionImportService(DatabaseContext context)
{
  private readonly DatabaseContext _context = context;

  public async Task ValidateAssignments(
    Guid? personId,
    IReadOnlyCollection<Guid> categoryIds,
    CancellationToken cancellationToken = default
  )
  {
    if (personId.HasValue && !await _context.Persons.AnyAsync(person =>
      person.Id == personId.Value && person.Deleted_at == null,
      cancellationToken
    ))
    {
      throw new NotFoundPersonException();
    }

    var requestedCategoryIds = categoryIds.ToHashSet();
    var activeCategoryCount = await _context.Categories.CountAsync(category =>
      requestedCategoryIds.Contains(category.Id) && category.Deleted_at == null,
      cancellationToken
    );

    if (activeCategoryCount != requestedCategoryIds.Count)
    {
      throw new NotFoundCategoryException();
    }
  }

  public async Task<TransactionImportModel> Create(
    Guid fileProcessingId,
    Guid transactionId,
    Guid? personId,
    IReadOnlyCollection<Guid> categoryIds,
    CancellationToken cancellationToken = default
  )
  {
    var now = DateTime.UtcNow;
    var model = new TransactionImportModel
    {
      Id = Guid.NewGuid(),
      FileProcessingId = fileProcessingId,
      TransactionId = transactionId,
      Created_at = now
    };

    _context.TransactionsImport.Add(model);

    if (personId.HasValue)
    {
      _context.TransactionsPerson.Add(new TransactionPersonModel
      {
        Id = Guid.NewGuid(),
        PersonId = personId.Value,
        TransactionId = transactionId,
        Created_at = now
      });
    }

    foreach (var categoryId in categoryIds)
    {
      _context.TransactionsCategory.Add(new TransactionCategoryModel
      {
        Id = Guid.NewGuid(),
        CategoryId = categoryId,
        TransactionId = transactionId,
        Created_at = now
      });
    }

    await _context.SaveChangesAsync(cancellationToken);

    return model;
  }
}
