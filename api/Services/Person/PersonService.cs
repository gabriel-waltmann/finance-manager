using api.Exceptions;
using api.Models.Database;
using api.Models.Person;
using api.Requests.Person;
using api.Responses.Person;
using Microsoft.EntityFrameworkCore;

namespace api.Services.Person;

public class PersonService(DatabaseContext context)
{
  private readonly DatabaseContext _context = context;

  private static PersonModel MapCreate(CreatePersonRequest dto)
  {
    return new PersonModel
    {
      Id = Guid.NewGuid(),
      Name = dto.Name,
      Email = dto.Email,
      PhoneNumber = dto.PhoneNumber,
      Created_at = DateTime.UtcNow
    };
  }

  public async Task<PersonModel> Get(Guid id)
  {
    return await _context.Persons.FirstOrDefaultAsync(person =>
      person.Id == id &&
      person.Deleted_at == null
    ) ?? throw new NotFoundPersonException();
  }

  public async Task<ListPersonResponse> List(ListPersonRequest request)
  {
    var query = _context.Persons
      .Where(person => request.WithDeleted || person.Deleted_at == null);

    if (!string.IsNullOrWhiteSpace(request.Search))
    {
      var searchPattern = $"%{EscapeLikePattern(request.Search)}%";

      query = query.Where(person =>
        EF.Functions.ILike(person.Name, searchPattern, "\\") ||
        EF.Functions.ILike(person.Email, searchPattern, "\\") ||
        EF.Functions.ILike(person.PhoneNumber, searchPattern, "\\")
      );
    }

    var total = await query.CountAsync();
    var orderedQuery = request.Order == "desc"
      ? query
        .OrderByDescending(person => person.Name)
        .ThenByDescending(person => person.Id)
      : query
        .OrderBy(person => person.Name)
        .ThenBy(person => person.Id);

    if (request.Page.HasValue && request.Limit.HasValue)
    {
      var page = request.Page.Value;
      var limit = request.Limit.Value;
      var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)limit);
      var persons = await orderedQuery
        .Skip((page - 1) * limit)
        .Take(limit)
        .ToListAsync();

      return new ListPersonResponse
      {
        Persons = persons,
        Page = page,
        Limit = limit,
        Total = total,
        TotalPages = totalPages
      };
    }

    return new ListPersonResponse
    {
      Persons = await orderedQuery.ToListAsync(),
      Page = 1,
      Limit = total,
      Total = total,
      TotalPages = total == 0 ? 0 : 1
    };
  }

  public async Task<PersonModel> Create(CreatePersonRequest dto)
  {
    var exists = await _context.Persons.AnyAsync(person =>
      person.Deleted_at == null &&
      person.Email == dto.Email
    );

    if (exists)
    {
      throw new ExistsPersonException();
    }

    var person = MapCreate(dto);

    _context.Persons.Add(person);

    await _context.SaveChangesAsync();

    return person;
  }

  public async Task Update(Guid id, UpdatePersonRequest request)
  {
    var person = await _context.Persons.FirstOrDefaultAsync(person =>
      person.Id == id &&
      person.Deleted_at == null
    ) ?? throw new NotFoundPersonException();

    var exists = await _context.Persons.AnyAsync(existing =>
      existing.Id != id &&
      existing.Deleted_at == null &&
      existing.Email == request.Email
    );

    if (exists)
    {
      throw new ExistsPersonException();
    }

    person.Name = request.Name;
    person.Email = request.Email;
    person.PhoneNumber = request.PhoneNumber;
    person.Updated_at = DateTime.UtcNow;

    await _context.SaveChangesAsync();
  }

  public async Task Delete(Guid id)
  {
    var person = await _context.Persons.FirstOrDefaultAsync(person =>
      person.Id == id &&
      person.Deleted_at == null
    );

    if (person == null)
    {
      return;
    }

    person.Deleted_at = DateTime.UtcNow;

    await _context.SaveChangesAsync();
  }

  private static string EscapeLikePattern(string value)
  {
    return value
      .Replace("\\", "\\\\", StringComparison.Ordinal)
      .Replace("%", "\\%", StringComparison.Ordinal)
      .Replace("_", "\\_", StringComparison.Ordinal);
  }
}
