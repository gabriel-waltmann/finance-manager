using api.Exceptions;
using api.Models.Database;
using api.Models.TransactionCategory;
using api.Requests.TransactionCategory;
using api.Responses.TransactionCategory;
using api.Services.Transaction;
using Microsoft.EntityFrameworkCore;

namespace api.Services.TransactionCategory;

public class TransactionCategoryService(
  DatabaseContext context,
  TransactionService transactionService
)
{
  private readonly DatabaseContext _context = context;
  private readonly TransactionService _transactionService = transactionService;

  public async Task<SetTransactionCategoriesResponse> Set(
    Guid transactionId,
    SetTransactionCategoriesRequest request
  )
  {
    await _transactionService.Get(transactionId);

    var requestedIds = request.CategoryIds.ToHashSet();
    var requestedCategories = await _context.Categories
      .Where(category => requestedIds.Contains(category.Id) && category.Deleted_at == null)
      .ToListAsync();

    if (requestedCategories.Count != requestedIds.Count)
    {
      throw new NotFoundCategoryException();
    }

    var currentLinks = await _context.TransactionsCategory
      .Where(link => link.TransactionId == transactionId && link.Deleted_at == null)
      .ToListAsync();
    var currentCategoryIds = currentLinks.Select(link => link.CategoryId).ToHashSet();
    var visibleCurrentIds = await _context.Categories
      .Where(category => currentCategoryIds.Contains(category.Id) && category.Deleted_at == null)
      .Select(category => category.Id)
      .ToListAsync();
    var now = DateTime.UtcNow;

    foreach (var link in currentLinks.Where(link =>
      visibleCurrentIds.Contains(link.CategoryId) &&
      !requestedIds.Contains(link.CategoryId)
    ))
    {
      link.Deleted_at = now;
    }

    foreach (var categoryId in requestedIds.Where(categoryId => !currentCategoryIds.Contains(categoryId)))
    {
      _context.TransactionsCategory.Add(new TransactionCategoryModel
      {
        Id = Guid.NewGuid(),
        TransactionId = transactionId,
        CategoryId = categoryId,
        Created_at = now
      });
    }

    try
    {
      await _context.SaveChangesAsync();
    }
    catch (DbUpdateException)
    {
      throw new ExistsTransactionCategoryException();
    }

    var transactionCategories = await _context.TransactionsCategory
      .Where(link => link.TransactionId == transactionId && link.Deleted_at == null)
      .OrderBy(link => link.Created_at)
      .ThenBy(link => link.Id)
      .ToListAsync();

    return new SetTransactionCategoriesResponse
    {
      TransactionCategories = transactionCategories,
      Categories = requestedCategories.OrderBy(category => category.Title).ToList()
    };
  }
}
