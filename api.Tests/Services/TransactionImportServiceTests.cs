using api.Exceptions;
using api.Models.Category;
using api.Models.Database;
using api.Models.Person;
using api.Models.Transaction;
using api.Services.TransactionImport;
using Microsoft.EntityFrameworkCore;

namespace api.Tests.Services;

public class TransactionImportServiceTests
{
  [Theory]
  [InlineData(false, false)]
  [InlineData(true, false)]
  [InlineData(false, true)]
  [InlineData(true, true)]
  public async Task Create_adds_requested_assignments(bool assignPerson, bool assignCategories)
  {
    await using var context = CreateContext();
    var person = CreatePerson();
    var categories = new[] { CreateCategory("Housing"), CreateCategory("Utilities") };
    var transaction = CreateTransaction();
    context.AddRange(person, transaction);
    context.AddRange(categories);
    await context.SaveChangesAsync();
    var service = new TransactionImportService(context);
    var categoryIds = assignCategories
      ? categories.Select(category => category.Id).ToList()
      : [];

    await service.ValidateAssignments(
      assignPerson ? person.Id : null,
      categoryIds
    );
    await service.Create(
      Guid.NewGuid(),
      transaction.Id,
      assignPerson ? person.Id : null,
      categoryIds
    );

    Assert.Single(await context.TransactionsImport.ToListAsync());
    Assert.Equal(assignPerson ? 1 : 0, await context.TransactionsPerson.CountAsync());
    Assert.Equal(assignCategories ? 2 : 0, await context.TransactionsCategory.CountAsync());
  }

  [Fact]
  public async Task ValidateAssignments_rejects_missing_or_deleted_person()
  {
    await using var context = CreateContext();
    var deletedPerson = CreatePerson();
    deletedPerson.Deleted_at = DateTime.UtcNow;
    context.Persons.Add(deletedPerson);
    await context.SaveChangesAsync();
    var service = new TransactionImportService(context);

    await Assert.ThrowsAsync<NotFoundPersonException>(() =>
      service.ValidateAssignments(deletedPerson.Id, []));
    await Assert.ThrowsAsync<NotFoundPersonException>(() =>
      service.ValidateAssignments(Guid.NewGuid(), []));
  }

  [Fact]
  public async Task ValidateAssignments_rejects_when_any_category_is_missing_or_deleted()
  {
    await using var context = CreateContext();
    var activeCategory = CreateCategory("Active");
    var deletedCategory = CreateCategory("Deleted");
    deletedCategory.Deleted_at = DateTime.UtcNow;
    context.Categories.AddRange(activeCategory, deletedCategory);
    await context.SaveChangesAsync();
    var service = new TransactionImportService(context);

    await Assert.ThrowsAsync<NotFoundCategoryException>(() => service.ValidateAssignments(
      null,
      [activeCategory.Id, deletedCategory.Id]
    ));
    await Assert.ThrowsAsync<NotFoundCategoryException>(() => service.ValidateAssignments(
      null,
      [activeCategory.Id, Guid.NewGuid()]
    ));
  }

  private static DatabaseContext CreateContext()
  {
    var options = new DbContextOptionsBuilder<DatabaseContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options;

    return new DatabaseContext(options);
  }

  private static PersonModel CreatePerson()
  {
    return new PersonModel
    {
      Id = Guid.NewGuid(),
      Name = "Person",
      Email = $"{Guid.NewGuid()}@example.com",
      PhoneNumber = "123456789",
      Created_at = DateTime.UtcNow
    };
  }

  private static CategoryModel CreateCategory(string title)
  {
    return new CategoryModel
    {
      Id = Guid.NewGuid(),
      Title = title,
      Created_at = DateTime.UtcNow
    };
  }

  private static TransactionModel CreateTransaction()
  {
    return new TransactionModel
    {
      Id = Guid.NewGuid(),
      Date = DateTime.UtcNow,
      Title = "Imported transaction",
      Amount = -10,
      Created_at = DateTime.UtcNow
    };
  }
}
