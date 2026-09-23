using api.Exceptions;
using api.Models.Category;
using api.Models.Database;
using api.Models.Person;
using api.Models.Transaction;
using api.Models.TransactionCategory;
using api.Models.TransactionPerson;
using api.Requests.Transaction;
using api.Services.Transaction;
using Microsoft.EntityFrameworkCore;

namespace api.Tests.Services;

public class AutoAssignTransactionsServiceTests
{
  [Fact]
  public async Task Assigns_person_and_adds_categories_to_every_date_match()
  {
    await using var context = CreateContext();
    var person = CreatePerson("Target");
    var existingCategory = CreateCategory("Existing");
    var addedCategory = CreateCategory("Added");
    var first = CreateTransaction(new DateTime(2026, 9, 1), "First");
    var second = CreateTransaction(new DateTime(2026, 9, 30, 23, 59, 0), "Second");
    var outside = CreateTransaction(new DateTime(2026, 10, 1), "Outside");
    context.AddRange(person, existingCategory, addedCategory, first, second, outside);
    context.TransactionsCategory.Add(CreateCategoryLink(first.Id, existingCategory.Id));
    await context.SaveChangesAsync();

    var response = await new TransactionService(context).AutoAssign(
      new AutoAssignTransactionsRequest
      {
        Filter = new AutoAssignTransactionFilterRequest
        {
          StartDate = new DateTime(2026, 9, 1),
          EndDate = new DateTime(2026, 9, 30)
        },
        PersonAction = "set",
        TargetPersonId = person.Id,
        CategoryAction = "add",
        TargetCategoryIds = [addedCategory.Id]
      }
    );

    Assert.Equal(2, response.MatchedCount);
    Assert.Equal(2, response.PersonChangedCount);
    Assert.Equal(2, response.CategoryChangedCount);
    Assert.Equal(2, await context.TransactionsPerson.CountAsync(link => link.Deleted_at == null));
    Assert.Equal(3, await context.TransactionsCategory.CountAsync(link => link.Deleted_at == null));
    Assert.Empty(await context.TransactionsPerson.Where(link =>
      link.TransactionId == outside.Id && link.Deleted_at == null).ToListAsync());
    Assert.Equal(2, await context.TransactionsCategory.CountAsync(link =>
      link.TransactionId == first.Id && link.Deleted_at == null));
  }

  [Fact]
  public async Task Replaces_person_and_visible_categories_while_preserving_deleted_category_history()
  {
    await using var context = CreateContext();
    var originalPerson = CreatePerson("Original");
    var targetPerson = CreatePerson("Target");
    var filterCategory = CreateCategory("Filter");
    var targetCategory = CreateCategory("Target");
    var deletedCategory = CreateCategory("Deleted");
    deletedCategory.Deleted_at = DateTime.UtcNow;
    var transaction = CreateTransaction(new DateTime(2026, 9, 10), "Match");
    var personLink = CreatePersonLink(transaction.Id, originalPerson.Id);
    var filterLink = CreateCategoryLink(transaction.Id, filterCategory.Id);
    var deletedCategoryLink = CreateCategoryLink(transaction.Id, deletedCategory.Id);
    context.AddRange(
      originalPerson,
      targetPerson,
      filterCategory,
      targetCategory,
      deletedCategory,
      transaction,
      personLink,
      filterLink,
      deletedCategoryLink
    );
    await context.SaveChangesAsync();

    var response = await new TransactionService(context).AutoAssign(
      new AutoAssignTransactionsRequest
      {
        Filter = new AutoAssignTransactionFilterRequest
        {
          PersonId = originalPerson.Id,
          CategoryId = filterCategory.Id
        },
        PersonAction = "set",
        TargetPersonId = targetPerson.Id,
        CategoryAction = "replace",
        TargetCategoryIds = [targetCategory.Id]
      }
    );

    Assert.Equal(1, response.MatchedCount);
    Assert.Equal(1, response.PersonChangedCount);
    Assert.Equal(1, response.CategoryChangedCount);
    Assert.Equal(targetPerson.Id, personLink.PersonId);
    Assert.NotNull(filterLink.Deleted_at);
    Assert.Null(deletedCategoryLink.Deleted_at);
    Assert.NotNull(await context.TransactionsCategory.SingleOrDefaultAsync(link =>
      link.TransactionId == transaction.Id &&
      link.CategoryId == targetCategory.Id &&
      link.Deleted_at == null));
  }

  [Fact]
  public async Task Clear_counts_only_transactions_that_changed_and_excludes_deleted_transactions()
  {
    await using var context = CreateContext();
    var person = CreatePerson("Person");
    var category = CreateCategory("Category");
    var assigned = CreateTransaction(new DateTime(2026, 9, 1), "Assigned");
    var empty = CreateTransaction(new DateTime(2026, 9, 2), "Empty");
    var deleted = CreateTransaction(new DateTime(2026, 9, 3), "Deleted");
    deleted.Deleted_at = DateTime.UtcNow;
    var personLink = CreatePersonLink(assigned.Id, person.Id);
    var categoryLink = CreateCategoryLink(assigned.Id, category.Id);
    var deletedPersonLink = CreatePersonLink(deleted.Id, person.Id);
    context.AddRange(
      person,
      category,
      assigned,
      empty,
      deleted,
      personLink,
      categoryLink,
      deletedPersonLink
    );
    await context.SaveChangesAsync();

    var response = await new TransactionService(context).AutoAssign(
      new AutoAssignTransactionsRequest
      {
        Filter = new AutoAssignTransactionFilterRequest
        {
          StartDate = new DateTime(2026, 9, 1)
        },
        PersonAction = "clear",
        CategoryAction = "clear"
      }
    );

    Assert.Equal(2, response.MatchedCount);
    Assert.Equal(1, response.PersonChangedCount);
    Assert.Equal(1, response.CategoryChangedCount);
    Assert.NotNull(personLink.Deleted_at);
    Assert.NotNull(categoryLink.Deleted_at);
    Assert.Null(deletedPersonLink.Deleted_at);
  }

  [Fact]
  public async Task Missing_assignment_target_leaves_existing_assignments_unchanged()
  {
    await using var context = CreateContext();
    var person = CreatePerson("Existing");
    var transaction = CreateTransaction(new DateTime(2026, 9, 1), "Transaction");
    var personLink = CreatePersonLink(transaction.Id, person.Id);
    context.AddRange(person, transaction, personLink);
    await context.SaveChangesAsync();

    await Assert.ThrowsAsync<NotFoundPersonException>(() =>
      new TransactionService(context).AutoAssign(new AutoAssignTransactionsRequest
      {
        Filter = new AutoAssignTransactionFilterRequest { Unassigned = false, StartDate = transaction.Date },
        PersonAction = "set",
        TargetPersonId = Guid.NewGuid()
      })
    );

    Assert.Equal(person.Id, personLink.PersonId);
    Assert.Null(personLink.Updated_at);
    Assert.Null(personLink.Deleted_at);

    await Assert.ThrowsAsync<NotFoundCategoryException>(() =>
      new TransactionService(context).AutoAssign(new AutoAssignTransactionsRequest
      {
        Filter = new AutoAssignTransactionFilterRequest { StartDate = transaction.Date },
        CategoryAction = "add",
        TargetCategoryIds = [Guid.NewGuid()]
      })
    );

    Assert.Empty(await context.TransactionsCategory.ToListAsync());
  }

  [Fact]
  public async Task Updates_all_matches_without_preview_page_limits()
  {
    await using var context = CreateContext();
    var person = CreatePerson("Target");
    var transactions = Enumerable.Range(1, 25)
      .Select(index => CreateTransaction(new DateTime(2026, 9, 1), $"Transaction {index}"))
      .ToList();
    context.Persons.Add(person);
    context.Transactions.AddRange(transactions);
    await context.SaveChangesAsync();

    var response = await new TransactionService(context).AutoAssign(
      new AutoAssignTransactionsRequest
      {
        Filter = new AutoAssignTransactionFilterRequest
        {
          StartDate = new DateTime(2026, 9, 1),
          EndDate = new DateTime(2026, 9, 1)
        },
        PersonAction = "set",
        TargetPersonId = person.Id
      }
    );

    Assert.Equal(25, response.MatchedCount);
    Assert.Equal(25, response.PersonChangedCount);
    Assert.Equal(25, await context.TransactionsPerson.CountAsync(link => link.Deleted_at == null));
  }

  private static DatabaseContext CreateContext()
  {
    var options = new DbContextOptionsBuilder<DatabaseContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options;
    return new DatabaseContext(options);
  }

  private static TransactionModel CreateTransaction(DateTime date, string title)
  {
    return new TransactionModel
    {
      Id = Guid.NewGuid(),
      Date = date,
      Title = title,
      Amount = 10,
      Created_at = DateTime.UtcNow
    };
  }

  private static PersonModel CreatePerson(string name)
  {
    return new PersonModel
    {
      Id = Guid.NewGuid(),
      Name = name,
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

  private static TransactionPersonModel CreatePersonLink(Guid transactionId, Guid personId)
  {
    return new TransactionPersonModel
    {
      Id = Guid.NewGuid(),
      TransactionId = transactionId,
      PersonId = personId,
      Created_at = DateTime.UtcNow
    };
  }

  private static TransactionCategoryModel CreateCategoryLink(Guid transactionId, Guid categoryId)
  {
    return new TransactionCategoryModel
    {
      Id = Guid.NewGuid(),
      TransactionId = transactionId,
      CategoryId = categoryId,
      Created_at = DateTime.UtcNow
    };
  }
}
