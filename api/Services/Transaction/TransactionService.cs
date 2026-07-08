using api.Requests.Transaction;
using api.Models.Transaction;
using api.Models.Database;
using api.Models.Person;
using api.Models.TransactionPerson;
using api.Responses.Transaction;
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

  public async Task<GetTransactionResponse> GetWithTransactionPerson(Guid id)
  {
    var transaction = await Get(id);
    var transactionPerson = await GetTransactionPerson(transaction.Id, false);
    var person = transactionPerson == null
      ? null
      : await GetPerson(transactionPerson.PersonId, false);

    return new GetTransactionResponse
    {
      Transaction = transaction,
      TransactionPerson = transactionPerson,
      Person = person
    };
  }

  public async Task<List<TransactionModel>> List(bool withDeleted)
  {
    return await _context.Transactions
      .Where(transaction => withDeleted || transaction.Deleted_at == null)
      .ToListAsync();
  }

  // TODO: refactor to use one sql query 
  public async Task<ListTransactionResponse> ListWithTransactionPerson(ListTransactionRequest request)
  {
    var withDeleted = request.WithDeleted == "true";
    var query = _context.Transactions
      .Where(transaction => withDeleted || transaction.Deleted_at == null);

    if (request.StartDate.HasValue)
    {
      var startDate = request.StartDate.Value.Date;

      query = query.Where(transaction => transaction.Date >= startDate);
    }

    if (request.EndDate.HasValue)
    {
      var nextEndDate = request.EndDate.Value.Date.AddDays(1);

      query = query.Where(transaction => transaction.Date < nextEndDate);
    }

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
        )
      );
    }

    if (request.PersonId.HasValue)
    {
      var personId = request.PersonId.Value;

      query = query.Where(transaction =>
        _context.TransactionsPerson.Any(transactionPerson =>
          transactionPerson.TransactionId == transaction.Id &&
          transactionPerson.Deleted_at == null &&
          transactionPerson.PersonId == personId &&
          (withDeleted || _context.Persons.Any(person =>
            person.Id == transactionPerson.PersonId &&
            person.Deleted_at == null
          ))
        )
      );
    }
    else if (request.Unassigned)
    {
      query = query.Where(transaction =>
        !_context.TransactionsPerson.Any(transactionPerson =>
          transactionPerson.TransactionId == transaction.Id &&
          transactionPerson.Deleted_at == null
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

    return new ListTransactionResponse
    {
      Transactions = transactions.Select(transaction => new GetTransactionResponse
      {
        Transaction = transaction,
        TransactionPerson = transactionPersonByTransactionId.GetValueOrDefault(transaction.Id),
        Person = GetPersonFromTransactionPerson(
          transactionPersonByTransactionId.GetValueOrDefault(transaction.Id),
          personById
        )
      }).ToList(),
      Page = request.Page,
      Limit = request.Limit,
      Total = total,
      TotalPages = totalPages
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

  private static PersonModel? GetPersonFromTransactionPerson(
    TransactionPersonModel? transactionPerson,
    Dictionary<Guid, PersonModel> personById
  )
  {
    return transactionPerson == null
      ? null
      : personById.GetValueOrDefault(transactionPerson.PersonId);
  }

  private static string EscapeLikePattern(string value)
  {
    return value
      .Replace("\\", "\\\\")
      .Replace("%", "\\%")
      .Replace("_", "\\_");
  }
}
