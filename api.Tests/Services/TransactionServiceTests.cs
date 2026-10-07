using api.Models.Category;
using api.Models.Database;
using api.Models.Transaction;
using api.Models.TransactionCategory;
using api.Requests.Transaction;
using api.Services.Transaction;
using Microsoft.EntityFrameworkCore;

namespace api.Tests.Services;

public class TransactionServiceTests
{
  [Fact]
  public async Task Category_filter_requires_every_selected_category_and_handles_uncategorized()
  {
    await using var context = CreateContext();
    var firstCategory = CreateCategory("First");
    var secondCategory = CreateCategory("Second");
    var otherCategory = CreateCategory("Other");
    var deletedCategory = CreateCategory("Deleted");
    deletedCategory.Deleted_at = DateTime.UtcNow;

    var firstMatch = CreateTransaction("First match", 1);
    var secondMatch = CreateTransaction("Second match", 2);
    var bothMatch = CreateTransaction("Both match", 3);
    var bothWithExtra = CreateTransaction("Both with extra", 4);
    var partialWithDeletedLink = CreateTransaction("Partial with deleted link", 5);
    var noLinks = CreateTransaction("No links", 6);
    var deletedLinkOnly = CreateTransaction("Deleted link", 7);
    var deletedCategoryOnly = CreateTransaction("Deleted category", 8);
    var unrelated = CreateTransaction("Unrelated", 9);
    var deletedSecondLink = CreateLink(partialWithDeletedLink.Id, secondCategory.Id);
    deletedSecondLink.Deleted_at = DateTime.UtcNow;
    var deletedLink = CreateLink(deletedLinkOnly.Id, firstCategory.Id);
    deletedLink.Deleted_at = DateTime.UtcNow;

    context.AddRange(
      firstCategory,
      secondCategory,
      otherCategory,
      deletedCategory,
      firstMatch,
      secondMatch,
      bothMatch,
      bothWithExtra,
      partialWithDeletedLink,
      noLinks,
      deletedLinkOnly,
      deletedCategoryOnly,
      unrelated,
      CreateLink(firstMatch.Id, firstCategory.Id),
      CreateLink(secondMatch.Id, secondCategory.Id),
      CreateLink(bothMatch.Id, firstCategory.Id),
      CreateLink(bothMatch.Id, secondCategory.Id),
      CreateLink(bothWithExtra.Id, firstCategory.Id),
      CreateLink(bothWithExtra.Id, secondCategory.Id),
      CreateLink(bothWithExtra.Id, otherCategory.Id),
      CreateLink(partialWithDeletedLink.Id, firstCategory.Id),
      deletedSecondLink,
      deletedLink,
      CreateLink(deletedCategoryOnly.Id, deletedCategory.Id),
      CreateLink(unrelated.Id, otherCategory.Id)
    );
    await context.SaveChangesAsync();

    var service = new TransactionService(context);
    var firstCategoryPage = await service.ListWithAssignments(new ListTransactionRequest
    {
      CategoryIds = [firstCategory.Id, secondCategory.Id],
      Order = "asc",
      Page = 1,
      Limit = 1
    });
    var secondCategoryPage = await service.ListWithAssignments(new ListTransactionRequest
    {
      CategoryIds = [firstCategory.Id, secondCategory.Id],
      Order = "asc",
      Page = 2,
      Limit = 1
    });
    var uncategorized = await service.ListWithAssignments(new ListTransactionRequest
    {
      Uncategorized = true,
      Order = "asc"
    });

    Assert.Equal(2, firstCategoryPage.Total);
    Assert.Equal(2, firstCategoryPage.TotalPages);
    Assert.Equal(2, secondCategoryPage.Total);
    var categorizedIds = firstCategoryPage.Transactions
      .Concat(secondCategoryPage.Transactions)
      .Select(item => item.Transaction.Id)
      .ToList();
    Assert.Equal(2, categorizedIds.Count);
    Assert.Equal(2, categorizedIds.Distinct().Count());
    Assert.Equal([bothMatch.Id, bothWithExtra.Id], categorizedIds);
    Assert.DoesNotContain(firstMatch.Id, categorizedIds);
    Assert.DoesNotContain(secondMatch.Id, categorizedIds);
    Assert.DoesNotContain(partialWithDeletedLink.Id, categorizedIds);

    Assert.Equal(3, uncategorized.Total);
    Assert.Equal(
      [noLinks.Id, deletedLinkOnly.Id, deletedCategoryOnly.Id],
      uncategorized.Transactions.Select(item => item.Transaction.Id)
    );
  }

  private static DatabaseContext CreateContext()
  {
    var options = new DbContextOptionsBuilder<DatabaseContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options;
    return new DatabaseContext(options);
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

  private static TransactionModel CreateTransaction(string title, int day)
  {
    var date = new DateTime(2026, 10, day);
    return new TransactionModel
    {
      Id = Guid.NewGuid(),
      Date = date,
      Title = title,
      Amount = day,
      Created_at = date
    };
  }

  private static TransactionCategoryModel CreateLink(Guid transactionId, Guid categoryId)
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
