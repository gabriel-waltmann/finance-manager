using api.Exceptions;
using api.Models.Category;
using api.Models.Database;
using api.Models.Transaction;
using api.Models.TransactionCategory;
using api.Normalization.Category;
using api.Requests.Category;
using api.Requests.TransactionCategory;
using api.Services.Category;
using api.Services.Transaction;
using api.Services.TransactionCategory;
using Microsoft.EntityFrameworkCore;

namespace api.Tests.Services;

public class CategoryServiceTests
{
  [Fact]
  public async Task Category_crud_persists_normalized_fields_and_reuses_soft_deleted_title()
  {
    await using var context = CreateContext();
    var service = new CategoryService(context);
    var createNormalizer = new CreateCategoryRequestNormalizer();
    var updateNormalizer = new UpdateCategoryRequestNormalizer();

    var create = new CreateCategoryRequest
    {
      Title = "  Housing  ",
      Description = "   "
    };
    createNormalizer.Normalize(create);
    var category = await service.Create(create);

    Assert.Equal("Housing", category.Title);
    Assert.Null(category.Description);
    await Assert.ThrowsAsync<ExistsCategoryException>(() => service.Create(
      new CreateCategoryRequest { Title = "Housing" }
    ));

    var update = new UpdateCategoryRequest
    {
      Title = "  Home  ",
      Description = "  Rent and utilities  "
    };
    updateNormalizer.Normalize(update);
    await service.Update(category.Id, update);

    Assert.Equal("Home", category.Title);
    Assert.Equal("Rent and utilities", category.Description);

    await service.Delete(category.Id);
    var replacementRequest = new CreateCategoryRequest { Title = "Home" };
    createNormalizer.Normalize(replacementRequest);
    var replacement = await service.Create(replacementRequest);

    Assert.NotEqual(category.Id, replacement.Id);
    Assert.NotNull(category.Deleted_at);
  }

  [Fact]
  public async Task Set_adds_multiple_categories_and_soft_deletes_removed_visible_links()
  {
    await using var context = CreateContext();
    var transaction = CreateTransaction();
    var first = CreateCategory("First");
    var second = CreateCategory("Second");
    context.AddRange(transaction, first, second);
    await context.SaveChangesAsync();
    var service = CreateTransactionCategoryService(context);

    var initial = await service.Set(transaction.Id, new SetTransactionCategoriesRequest
    {
      CategoryIds = [first.Id, second.Id]
    });

    Assert.Equal(2, initial.Categories.Count);
    Assert.Equal(2, initial.TransactionCategories.Count);

    var replacement = await service.Set(transaction.Id, new SetTransactionCategoriesRequest
    {
      CategoryIds = [second.Id]
    });
    var firstLink = await context.TransactionsCategory.SingleAsync(link =>
      link.TransactionId == transaction.Id && link.CategoryId == first.Id
    );

    Assert.NotNull(firstLink.Deleted_at);
    Assert.Single(replacement.Categories);
    Assert.Equal(second.Id, replacement.Categories[0].Id);
    Assert.Single(replacement.TransactionCategories);
  }

  [Fact]
  public async Task Set_validates_all_ids_before_changing_assignments()
  {
    await using var context = CreateContext();
    var transaction = CreateTransaction();
    var category = CreateCategory("Existing");
    var link = CreateLink(transaction.Id, category.Id);
    context.AddRange(transaction, category, link);
    await context.SaveChangesAsync();
    var service = CreateTransactionCategoryService(context);

    await Assert.ThrowsAsync<NotFoundCategoryException>(() => service.Set(
      transaction.Id,
      new SetTransactionCategoriesRequest { CategoryIds = [Guid.NewGuid()] }
    ));

    Assert.Null(link.Deleted_at);
    Assert.Single(await context.TransactionsCategory.Where(item => item.Deleted_at == null).ToListAsync());
  }

  [Fact]
  public async Task Set_preserves_links_to_soft_deleted_categories()
  {
    await using var context = CreateContext();
    var transaction = CreateTransaction();
    var category = CreateCategory("Deleted");
    category.Deleted_at = DateTime.UtcNow;
    var link = CreateLink(transaction.Id, category.Id);
    context.AddRange(transaction, category, link);
    await context.SaveChangesAsync();
    var service = CreateTransactionCategoryService(context);

    var response = await service.Set(transaction.Id, new SetTransactionCategoriesRequest
    {
      CategoryIds = []
    });

    Assert.Null(link.Deleted_at);
    Assert.Empty(response.Categories);
    Assert.Single(response.TransactionCategories);
    Assert.Equal(link.Id, response.TransactionCategories[0].Id);
  }

  private static DatabaseContext CreateContext()
  {
    var options = new DbContextOptionsBuilder<DatabaseContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options;

    return new DatabaseContext(options);
  }

  private static TransactionCategoryService CreateTransactionCategoryService(
    DatabaseContext context
  )
  {
    return new TransactionCategoryService(context, new TransactionService(context));
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
      Title = "Test transaction",
      Amount = 10,
      Created_at = DateTime.UtcNow
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
