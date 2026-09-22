using api.Exceptions;
using api.Models.Database;
using api.Normalization.Person;
using api.Requests.Person;
using api.Services.Person;
using Microsoft.EntityFrameworkCore;

namespace api.Tests.Services;

public class PersonServiceTests
{
  [Fact]
  public async Task Create_and_update_persist_canonical_values_and_compare_canonical_emails()
  {
    await using var context = CreateContext();
    var service = new PersonService(context);
    var createNormalizer = new CreatePersonRequestNormalizer();
    var updateNormalizer = new UpdatePersonRequestNormalizer();
    var createFirst = new CreatePersonRequest
    {
      Name = "  Jos\u0065\u0301  ",
      Email = " FIRST@EXAMPLE.COM ",
      PhoneNumber = "  +55 11 99999-9999  "
    };
    createNormalizer.Normalize(createFirst);
    var first = await service.Create(createFirst);

    Assert.Equal("José", first.Name);
    Assert.Equal("first@example.com", first.Email);
    Assert.Equal("+55 11 99999-9999", first.PhoneNumber);

    var duplicate = new CreatePersonRequest
    {
      Name = "Duplicate",
      Email = "First@Example.Com",
      PhoneNumber = "123"
    };
    createNormalizer.Normalize(duplicate);
    await Assert.ThrowsAsync<ExistsPersonException>(() => service.Create(duplicate));

    var update = new UpdatePersonRequest
    {
      Name = " Updated ",
      Email = " UPDATED@EXAMPLE.COM ",
      PhoneNumber = " 456 "
    };
    updateNormalizer.Normalize(update);
    await service.Update(first.Id, update);

    Assert.Equal("Updated", first.Name);
    Assert.Equal("updated@example.com", first.Email);
    Assert.Equal("456", first.PhoneNumber);
  }

  private static DatabaseContext CreateContext()
  {
    var options = new DbContextOptionsBuilder<DatabaseContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options;

    return new DatabaseContext(options);
  }
}
