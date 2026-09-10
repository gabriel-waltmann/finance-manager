using api.Models.Category;
using api.Models.Database;
using api.Models.Transaction;
using api.Models.TransactionCategory;
using api.Requests.Dashboard;
using api.Services.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace api.Tests.Services;

public class DashboardServiceTests
{
  [Fact]
  public async Task Category_filter_applies_to_aggregates_and_pagination_without_duplicates()
  {
    await using var context = CreateContext();
    var housing = CreateCategory("Housing");
    var recurring = CreateCategory("Recurring");
    var groceries = CreateCategory("Groceries");
    var januaryRent = CreateTransaction("Rent", new DateTime(2026, 1, 10), -100);
    var februaryRent = CreateTransaction("RENT", new DateTime(2026, 2, 10), -120);
    var power = CreateTransaction("Power", new DateTime(2026, 2, 15), -40);
    var food = CreateTransaction("Food", new DateTime(2026, 2, 20), -30);

    context.AddRange(housing, recurring, groceries, januaryRent, februaryRent, power, food);
    context.AddRange(
      CreateLink(januaryRent.Id, housing.Id),
      CreateLink(januaryRent.Id, recurring.Id),
      CreateLink(februaryRent.Id, housing.Id),
      CreateLink(power.Id, housing.Id),
      CreateLink(food.Id, groceries.Id)
    );
    await context.SaveChangesAsync();

    var response = await new DashboardService(context).Get(new GetDashboardRequest
    {
      CategoryId = housing.Id,
      Page = 1,
      Limit = 1
    });

    Assert.Equal(260, response.TotalAmount);
    Assert.Equal(2, response.Total);
    Assert.Equal(2, response.TotalPages);
    var topItem = Assert.Single(response.TopItems);
    Assert.Equal("Rent", topItem.Title, ignoreCase: true);
    Assert.Equal(220, topItem.TotalAmount);
    Assert.Equal(2, topItem.TransactionCount);
    var fixedSpend = Assert.Single(response.FixedSpends);
    Assert.Equal("RENT", fixedSpend.Title);
    Assert.Equal(2, fixedSpend.MonthCount);
    Assert.Equal(120, fixedSpend.LastAmount);
  }

  [Fact]
  public async Task Uncategorized_filter_ignores_deleted_links_and_deleted_categories()
  {
    await using var context = CreateContext();
    var activeCategory = CreateCategory("Active");
    var deletedCategory = CreateCategory("Deleted");
    deletedCategory.Deleted_at = DateTime.UtcNow;
    var assigned = CreateTransaction("Assigned", new DateTime(2026, 1, 1), -10);
    var noLink = CreateTransaction("No link", new DateTime(2026, 1, 2), -20);
    var deletedLink = CreateTransaction("Deleted link", new DateTime(2026, 1, 3), -30);
    var deletedCategoryLink = CreateTransaction("Deleted category", new DateTime(2026, 1, 4), -40);
    var inactiveLink = CreateLink(deletedLink.Id, activeCategory.Id);
    inactiveLink.Deleted_at = DateTime.UtcNow;

    context.AddRange(
      activeCategory,
      deletedCategory,
      assigned,
      noLink,
      deletedLink,
      deletedCategoryLink,
      CreateLink(assigned.Id, activeCategory.Id),
      inactiveLink,
      CreateLink(deletedCategoryLink.Id, deletedCategory.Id)
    );
    await context.SaveChangesAsync();

    var response = await new DashboardService(context).Get(new GetDashboardRequest
    {
      Uncategorized = true
    });

    Assert.Equal(90, response.TotalAmount);
    Assert.Equal(3, response.Total);
    Assert.DoesNotContain(response.TopItems, item => item.Title == "Assigned");
    Assert.Contains(response.TopItems, item => item.Title == "No link");
    Assert.Contains(response.TopItems, item => item.Title == "Deleted link");
    Assert.Contains(response.TopItems, item => item.Title == "Deleted category");
  }

  [Fact]
  public async Task Category_filter_requires_an_active_link_to_an_active_category()
  {
    await using var context = CreateContext();
    var category = CreateCategory("Active");
    var active = CreateTransaction("Active link", new DateTime(2026, 1, 1), -10);
    var deletedLink = CreateTransaction("Deleted link", new DateTime(2026, 1, 2), -20);
    var inactiveLink = CreateLink(deletedLink.Id, category.Id);
    inactiveLink.Deleted_at = DateTime.UtcNow;

    context.AddRange(category, active, deletedLink, CreateLink(active.Id, category.Id), inactiveLink);
    await context.SaveChangesAsync();

    var response = await new DashboardService(context).Get(new GetDashboardRequest
    {
      CategoryId = category.Id
    });

    Assert.Equal(10, response.TotalAmount);
    Assert.Single(response.TopItems);
    Assert.Equal("Active link", response.TopItems[0].Title);

    category.Deleted_at = DateTime.UtcNow;
    await context.SaveChangesAsync();

    response = await new DashboardService(context).Get(new GetDashboardRequest
    {
      CategoryId = category.Id
    });

    Assert.Equal(0, response.TotalAmount);
    Assert.Empty(response.TopItems);
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

  private static TransactionModel CreateTransaction(string title, DateTime date, decimal amount)
  {
    return new TransactionModel
    {
      Id = Guid.NewGuid(),
      Title = title,
      Date = date,
      Amount = amount,
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
