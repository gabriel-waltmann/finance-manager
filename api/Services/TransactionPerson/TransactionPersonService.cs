using api.Exceptions;
using api.Models.Database;
using api.Models.TransactionPerson;
using api.Requests.TransactionPerson;
using api.Services.Person;
using api.Services.Transaction;
using Microsoft.EntityFrameworkCore;

namespace api.Services.TransactionPerson;

public class TransactionPersonService(
  DatabaseContext context,
  PersonService personService,
  TransactionService transactionService
)
{
  private readonly DatabaseContext _context = context;
  private readonly PersonService _personService = personService;
  private readonly TransactionService _transactionService = transactionService;

  private static TransactionPersonModel MapCreate(CreateTransactionPersonRequest dto)
  {
    return new TransactionPersonModel
    {
      Id = Guid.NewGuid(),
      PersonId = dto.PersonId,
      TransactionId = dto.TransactionId,
      Created_at = DateTime.UtcNow
    };
  }

  public async Task<TransactionPersonModel> Get(Guid id)
  {
    return await _context.TransactionsPerson.FirstOrDefaultAsync(transactionPerson =>
      transactionPerson.Id == id &&
      transactionPerson.Deleted_at == null
    ) ?? throw new NotFoundTransactionPersonException();
  }

  public async Task<List<TransactionPersonModel>> List(bool withDeleted)
  {
    return await _context.TransactionsPerson
      .Where(transactionPerson => withDeleted || transactionPerson.Deleted_at == null)
      .ToListAsync();
  }

  public async Task<TransactionPersonModel> Create(CreateTransactionPersonRequest dto)
  {
    await ValidateActivePerson(dto.PersonId);
    await ValidateActiveTransaction(dto.TransactionId);
    await ValidateUniqueTransaction(dto.TransactionId);

    var transactionPerson = MapCreate(dto);

    _context.TransactionsPerson.Add(transactionPerson);

    await _context.SaveChangesAsync();

    return transactionPerson;
  }

  public async Task Update(Guid id, UpdateTransactionPersonRequest request)
  {
    var transactionPerson = await _context.TransactionsPerson.FirstOrDefaultAsync(transactionPerson =>
      transactionPerson.Id == id &&
      transactionPerson.Deleted_at == null
    )
      ?? throw new NotFoundTransactionPersonException();

    await ValidateActivePerson(request.PersonId);
    await ValidateActiveTransaction(request.TransactionId);
    await ValidateUniqueTransaction(request.TransactionId, id);

    transactionPerson.PersonId = request.PersonId;
    transactionPerson.TransactionId = request.TransactionId;
    transactionPerson.Updated_at = DateTime.UtcNow;

    await _context.SaveChangesAsync();
  }

  public async Task Delete(Guid id)
  {
    var transactionPerson = await _context.TransactionsPerson.FirstOrDefaultAsync(transactionPerson =>
      transactionPerson.Id == id &&
      transactionPerson.Deleted_at == null
    );

    if (transactionPerson == null)
    {
      return;
    }

    transactionPerson.Deleted_at = DateTime.UtcNow;

    await _context.SaveChangesAsync();
  }

  private async Task ValidateActivePerson(Guid personId)
  {
    await _personService.Get(personId);
  }

  private async Task ValidateActiveTransaction(Guid transactionId)
  {
    await _transactionService.Get(transactionId);
  }

  private async Task ValidateUniqueTransaction(Guid transactionId, Guid? currentId = null)
  {
    var exists = await _context.TransactionsPerson.AnyAsync(transactionPerson =>
      transactionPerson.TransactionId == transactionId &&
      transactionPerson.Deleted_at == null &&
      (currentId == null || transactionPerson.Id != currentId)
    );

    if (exists)
    {
      throw new ExistsTransactionPersonException();
    }
  }
}
