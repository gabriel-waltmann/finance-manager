using api.Requests.Transaction;
using api.Models.Transaction;
using api.Models.Database;
using api.Models.Person;
using api.Models.TransactionPerson;
using api.Models.Category;
using api.Models.TransactionCategory;
using api.Responses.Transaction;
using api.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Transaction;

// TODO: split all services to make files less complex
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

  public async Task<GetTransactionResponse> GetWithAssignments(Guid id)
  {
    var transaction = await Get(id);
    var transactionPerson = await GetTransactionPerson(transaction.Id, false);
    var person = transactionPerson == null
      ? null
      : await GetPerson(transactionPerson.PersonId, false);
    var transactionCategories = await GetTransactionCategories(transaction.Id, false);
    var categories = await GetCategories(transactionCategories, false);

    return new GetTransactionResponse
    {
      Transaction = transaction,
      TransactionPerson = transactionPerson,
      Person = person,
      TransactionCategories = transactionCategories,
      Categories = categories
    };
  }

  public async Task<List<TransactionModel>> List(bool withDeleted)
  {
    return await _context.Transactions
      .Where(transaction => withDeleted || transaction.Deleted_at == null)
      .ToListAsync();
  }

  // TODO: refactor to use one sql query 
  public async Task<ListTransactionResponse> ListWithAssignments(ListTransactionRequest request)
  {
    var withDeleted = request.WithDeleted;
    var query = _context.Transactions
      .Where(transaction => withDeleted || transaction.Deleted_at == null);

    query = ApplyFilters(
      query,
      request.StartDate,
      request.EndDate,
      request.PersonId,
      request.Unassigned,
      request.CategoryId,
      request.Uncategorized,
      withDeleted
    );

    if (!string.IsNullOrWhiteSpace(request.Search))
    {
      var searchPattern = $"%{EscapeLikePattern(request.Search)}%";

      query = query.Where(transaction =>
        EF.Functions.ILike(transaction.Title, searchPattern, "\\") ||
        _context.TransactionsPerson.Any(transactionPerson =>
          transactionPerson.TransactionId == transaction.Id &&
          (withDeleted || transactionPerson.Deleted_at == null) &&
          _context.Persons.Any(person =>
            person.Id == transactionPerson.PersonId &&
            (withDeleted || person.Deleted_at == null) &&
            EF.Functions.ILike(person.Name, searchPattern, "\\")
          )
        ) ||
        _context.TransactionsCategory.Any(transactionCategory =>
          transactionCategory.TransactionId == transaction.Id &&
          (withDeleted || transactionCategory.Deleted_at == null) &&
          _context.Categories.Any(category =>
            category.Id == transactionCategory.CategoryId &&
            (withDeleted || category.Deleted_at == null) &&
            EF.Functions.ILike(category.Title, searchPattern, "\\")
          )
        )
      );
    }

    var total = await query.CountAsync();
    var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)request.Limit);

    var orderedQuery = request.Order == "asc"
      ? query
        .OrderBy(transaction => transaction.Date)
        .ThenBy(transaction => transaction.Created_at)
      : query
        .OrderByDescending(transaction => transaction.Date)
        .ThenByDescending(transaction => transaction.Created_at);

    var transactions = await orderedQuery
      .Skip((request.Page - 1) * request.Limit)
      .Take(request.Limit)
      .ToListAsync();

    var transactionIds = transactions.Select(transaction => transaction.Id).ToList();

    var transactionPersons = await _context.TransactionsPerson
      .Where(transactionPerson =>
        transactionIds.Contains(transactionPerson.TransactionId) &&
        (withDeleted || transactionPerson.Deleted_at == null)
      )
      .OrderBy(transactionPerson => transactionPerson.Deleted_at == null ? 0 : 1)
      .ThenByDescending(transactionPerson => transactionPerson.Created_at)
      .ToListAsync();

    var transactionPersonByTransactionId = transactionPersons
      .GroupBy(transactionPerson => transactionPerson.TransactionId)
      .ToDictionary(group => group.Key, group => group.First());

    var personIds = transactionPersons.Select(transactionPerson => transactionPerson.PersonId).ToList();

    var persons = await _context.Persons
      .Where(person =>
        personIds.Contains(person.Id) &&
        (withDeleted || person.Deleted_at == null)
      )
      .ToListAsync();

    var personById = persons.ToDictionary(person => person.Id);

    var transactionCategories = await _context.TransactionsCategory
      .Where(transactionCategory =>
        transactionIds.Contains(transactionCategory.TransactionId) &&
        (withDeleted || transactionCategory.Deleted_at == null)
      )
      .OrderBy(transactionCategory => transactionCategory.Deleted_at == null ? 0 : 1)
      .ThenBy(transactionCategory => transactionCategory.Created_at)
      .ToListAsync();

    var transactionCategoriesByTransactionId = transactionCategories
      .GroupBy(transactionCategory => transactionCategory.TransactionId)
      .ToDictionary(group => group.Key, group => group.ToList());
    var categoryIds = transactionCategories
      .Select(transactionCategory => transactionCategory.CategoryId)
      .Distinct()
      .ToList();
    var categories = await _context.Categories
      .Where(category =>
        categoryIds.Contains(category.Id) &&
        (withDeleted || category.Deleted_at == null)
      )
      .ToListAsync();
    var categoryById = categories.ToDictionary(category => category.Id);

    return new ListTransactionResponse
    {
      Transactions = transactions.Select(transaction => new GetTransactionResponse
      {
        Transaction = transaction,
        TransactionPerson = transactionPersonByTransactionId.GetValueOrDefault(transaction.Id),
        Person = GetPersonFromTransactionPerson(
          transactionPersonByTransactionId.GetValueOrDefault(transaction.Id),
          personById
        ),
        TransactionCategories = transactionCategoriesByTransactionId
          .GetValueOrDefault(transaction.Id, []),
        Categories = GetCategoriesFromTransactionCategories(
          transactionCategoriesByTransactionId.GetValueOrDefault(transaction.Id, []),
          categoryById
        )
      }).ToList(),
      Page = request.Page,
      Limit = request.Limit,
      Total = total,
      TotalPages = totalPages
    };
  }

  public async Task<AutoAssignTransactionsResponse> AutoAssign(
    AutoAssignTransactionsRequest request
  )
  {
    var filter = request.Filter;
    var transactionIds = await ApplyFilters(
        _context.Transactions.Where(transaction => transaction.Deleted_at == null),
        filter.StartDate,
        filter.EndDate,
        filter.PersonId,
        filter.Unassigned,
        filter.CategoryId,
        filter.Uncategorized,
        false
      )
      .Select(transaction => transaction.Id)
      .ToListAsync();

    if (request.PersonAction == "set")
    {
      var personExists = await _context.Persons.AnyAsync(person =>
        person.Id == request.TargetPersonId && person.Deleted_at == null
      );

      if (!personExists)
      {
        throw new NotFoundPersonException();
      }
    }

    var targetCategoryIds = request.TargetCategoryIds.ToHashSet();

    if (request.CategoryAction is "add" or "replace")
    {
      var categoryCount = await _context.Categories.CountAsync(category =>
        targetCategoryIds.Contains(category.Id) && category.Deleted_at == null
      );

      if (categoryCount != targetCategoryIds.Count)
      {
        throw new NotFoundCategoryException();
      }
    }

    if (transactionIds.Count == 0)
    {
      return new AutoAssignTransactionsResponse
      {
        MatchedCount = 0,
        PersonChangedCount = 0,
        CategoryChangedCount = 0
      };
    }

    var transactionPersonByTransactionId = await _context.TransactionsPerson
      .Where(link => transactionIds.Contains(link.TransactionId) && link.Deleted_at == null)
      .ToDictionaryAsync(link => link.TransactionId);
    var categoryLinks = await _context.TransactionsCategory
      .Where(link => transactionIds.Contains(link.TransactionId) && link.Deleted_at == null)
      .ToListAsync();
    var linkedCategoryIds = categoryLinks
      .Select(link => link.CategoryId)
      .Distinct()
      .ToList();
    var visibleCategoryIds = await _context.Categories
      .Where(category => linkedCategoryIds.Contains(category.Id) && category.Deleted_at == null)
      .Select(category => category.Id)
      .ToHashSetAsync();
    var categoryLinksByTransactionId = categoryLinks
      .GroupBy(link => link.TransactionId)
      .ToDictionary(group => group.Key, group => group.ToList());
    var now = DateTime.UtcNow;
    var personChangedCount = 0;
    var categoryChangedCount = 0;

    foreach (var transactionId in transactionIds)
    {
      if (ApplyPersonAssignment(
        transactionId,
        transactionPersonByTransactionId.GetValueOrDefault(transactionId),
        request,
        now
      ))
      {
        personChangedCount++;
      }

      if (ApplyCategoryAssignment(
        transactionId,
        categoryLinksByTransactionId.GetValueOrDefault(transactionId, []),
        visibleCategoryIds,
        targetCategoryIds,
        request.CategoryAction,
        now
      ))
      {
        categoryChangedCount++;
      }
    }

    await _context.SaveChangesAsync();

    return new AutoAssignTransactionsResponse
    {
      MatchedCount = transactionIds.Count,
      PersonChangedCount = personChangedCount,
      CategoryChangedCount = categoryChangedCount
    };
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

  private IQueryable<TransactionModel> ApplyFilters(
    IQueryable<TransactionModel> query,
    DateTime? startDateValue,
    DateTime? endDateValue,
    Guid? personId,
    bool unassigned,
    Guid? categoryId,
    bool uncategorized,
    bool withDeleted
  )
  {
    if (startDateValue.HasValue)
    {
      var startDate = startDateValue.Value.Date;
      query = query.Where(transaction => transaction.Date >= startDate);
    }

    if (endDateValue.HasValue)
    {
      var nextEndDate = endDateValue.Value.Date.AddDays(1);
      query = query.Where(transaction => transaction.Date < nextEndDate);
    }

    if (personId.HasValue)
    {
      query = query.Where(transaction =>
        _context.TransactionsPerson.Any(transactionPerson =>
          transactionPerson.TransactionId == transaction.Id &&
          transactionPerson.Deleted_at == null &&
          transactionPerson.PersonId == personId.Value &&
          (withDeleted || _context.Persons.Any(person =>
            person.Id == transactionPerson.PersonId &&
            person.Deleted_at == null
          ))
        )
      );
    }
    else if (unassigned)
    {
      query = query.Where(transaction =>
        !_context.TransactionsPerson.Any(transactionPerson =>
          transactionPerson.TransactionId == transaction.Id &&
          transactionPerson.Deleted_at == null
        )
      );
    }

    if (categoryId.HasValue)
    {
      query = query.Where(transaction =>
        _context.TransactionsCategory.Any(transactionCategory =>
          transactionCategory.TransactionId == transaction.Id &&
          transactionCategory.Deleted_at == null &&
          transactionCategory.CategoryId == categoryId.Value &&
          (withDeleted || _context.Categories.Any(category =>
            category.Id == transactionCategory.CategoryId &&
            category.Deleted_at == null
          ))
        )
      );
    }
    else if (uncategorized)
    {
      query = query.Where(transaction =>
        !_context.TransactionsCategory.Any(transactionCategory =>
          transactionCategory.TransactionId == transaction.Id &&
          transactionCategory.Deleted_at == null &&
          _context.Categories.Any(category =>
            category.Id == transactionCategory.CategoryId &&
            (withDeleted || category.Deleted_at == null)
          )
        )
      );
    }

    return query;
  }

  private bool ApplyPersonAssignment(
    Guid transactionId,
    TransactionPersonModel? currentLink,
    AutoAssignTransactionsRequest request,
    DateTime now
  )
  {
    if (request.PersonAction == "unchanged")
    {
      return false;
    }

    if (request.PersonAction == "clear")
    {
      if (currentLink == null)
      {
        return false;
      }

      currentLink.Deleted_at = now;
      return true;
    }

    var targetPersonId = request.TargetPersonId!.Value;

    if (currentLink == null)
    {
      _context.TransactionsPerson.Add(new TransactionPersonModel
      {
        Id = Guid.NewGuid(),
        TransactionId = transactionId,
        PersonId = targetPersonId,
        Created_at = now
      });
      return true;
    }

    if (currentLink.PersonId == targetPersonId)
    {
      return false;
    }

    currentLink.PersonId = targetPersonId;
    currentLink.Updated_at = now;
    return true;
  }

  private bool ApplyCategoryAssignment(
    Guid transactionId,
    List<TransactionCategoryModel> currentLinks,
    HashSet<Guid> visibleCategoryIds,
    HashSet<Guid> targetCategoryIds,
    string action,
    DateTime now
  )
  {
    if (action == "unchanged")
    {
      return false;
    }

    var currentCategoryIds = currentLinks.Select(link => link.CategoryId).ToHashSet();
    var linksToRemove = action is "replace" or "clear"
      ? currentLinks.Where(link =>
        visibleCategoryIds.Contains(link.CategoryId) &&
        !targetCategoryIds.Contains(link.CategoryId)
      ).ToList()
      : [];
    var categoryIdsToAdd = action is "add" or "replace"
      ? targetCategoryIds.Where(categoryId => !currentCategoryIds.Contains(categoryId)).ToList()
      : [];

    if (linksToRemove.Count == 0 && categoryIdsToAdd.Count == 0)
    {
      return false;
    }

    foreach (var link in linksToRemove)
    {
      link.Deleted_at = now;
    }

    foreach (var categoryId in categoryIdsToAdd)
    {
      _context.TransactionsCategory.Add(new TransactionCategoryModel
      {
        Id = Guid.NewGuid(),
        TransactionId = transactionId,
        CategoryId = categoryId,
        Created_at = now
      });
    }

    return true;
  }

  private async Task<TransactionPersonModel?> GetTransactionPerson(Guid transactionId, bool withDeleted)
  {
    return await _context.TransactionsPerson
      .Where(transactionPerson =>
        transactionPerson.TransactionId == transactionId &&
        (withDeleted || transactionPerson.Deleted_at == null)
      )
      .OrderBy(transactionPerson => transactionPerson.Deleted_at == null ? 0 : 1)
      .ThenByDescending(transactionPerson => transactionPerson.Created_at)
      .FirstOrDefaultAsync();
  }

  private async Task<PersonModel?> GetPerson(Guid personId, bool withDeleted)
  {
    return await _context.Persons.FirstOrDefaultAsync(person =>
      person.Id == personId &&
      (withDeleted || person.Deleted_at == null)
    );
  }

  private async Task<List<TransactionCategoryModel>> GetTransactionCategories(
    Guid transactionId,
    bool withDeleted
  )
  {
    return await _context.TransactionsCategory
      .Where(transactionCategory =>
        transactionCategory.TransactionId == transactionId &&
        (withDeleted || transactionCategory.Deleted_at == null)
      )
      .OrderBy(transactionCategory => transactionCategory.Deleted_at == null ? 0 : 1)
      .ThenBy(transactionCategory => transactionCategory.Created_at)
      .ToListAsync();
  }

  private async Task<List<CategoryModel>> GetCategories(
    List<TransactionCategoryModel> transactionCategories,
    bool withDeleted
  )
  {
    var categoryIds = transactionCategories
      .Select(transactionCategory => transactionCategory.CategoryId)
      .Distinct()
      .ToList();

    return await _context.Categories
      .Where(category =>
        categoryIds.Contains(category.Id) &&
        (withDeleted || category.Deleted_at == null)
      )
      .OrderBy(category => category.Title)
      .ThenBy(category => category.Id)
      .ToListAsync();
  }

  private static PersonModel? GetPersonFromTransactionPerson(
    TransactionPersonModel? transactionPerson,
    Dictionary<Guid, PersonModel> personById
  )
  {
    return transactionPerson == null
      ? null
      : personById.GetValueOrDefault(transactionPerson.PersonId);
  }

  private static List<CategoryModel> GetCategoriesFromTransactionCategories(
    List<TransactionCategoryModel> transactionCategories,
    Dictionary<Guid, CategoryModel> categoryById
  )
  {
    return transactionCategories
      .Select(transactionCategory => categoryById.GetValueOrDefault(transactionCategory.CategoryId))
      .Where(category => category != null)
      .DistinctBy(category => category!.Id)
      .OrderBy(category => category!.Title)
      .Select(category => category!)
      .ToList();
  }

  private static string EscapeLikePattern(string value)
  {
    return value
      .Replace("\\", "\\\\")
      .Replace("%", "\\%")
      .Replace("_", "\\_");
  }
}
