using api.Exceptions;
using api.Models.Database;
using api.Models.Person;
using api.Requests.Person;
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

  public async Task<List<PersonModel>> List(bool withDeleted)
  {
    return await _context.Persons
      .Where(person => withDeleted || person.Deleted_at == null)
      .ToListAsync();
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
}
