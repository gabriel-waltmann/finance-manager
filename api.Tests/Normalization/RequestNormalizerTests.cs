using System.Reflection;
using System.Runtime.CompilerServices;
using api.Normalization;
using api.Normalization.Category;
using api.Normalization.Dashboard;
using api.Normalization.Person;
using api.Normalization.Transaction;
using api.Requests.Category;
using api.Requests.Dashboard;
using api.Requests.Person;
using api.Requests.Transaction;

namespace api.Tests.Normalization;

public class RequestNormalizerTests
{
  [Fact]
  public void Person_requests_are_normalized_idempotently()
  {
    var create = new CreatePersonRequest
    {
      Name = "  Jose\u0301  ",
      Email = " USER@EXAMPLE.COM ",
      PhoneNumber = " 123 "
    };
    var update = new UpdatePersonRequest
    {
      Name = " Updated ",
      Email = " UPDATED@EXAMPLE.COM ",
      PhoneNumber = " 456 "
    };
    var list = new ListPersonRequest { Search = "  Jose\u0301  ", Order = " DESC " };
    var createNormalizer = new CreatePersonRequestNormalizer();
    var updateNormalizer = new UpdatePersonRequestNormalizer();
    var listNormalizer = new ListPersonRequestNormalizer();

    createNormalizer.Normalize(create);
    updateNormalizer.Normalize(update);
    listNormalizer.Normalize(list);
    createNormalizer.Normalize(create);
    updateNormalizer.Normalize(update);
    listNormalizer.Normalize(list);

    Assert.Equal("José", create.Name);
    Assert.Equal("user@example.com", create.Email);
    Assert.Equal("123", create.PhoneNumber);
    Assert.Equal("Updated", update.Name);
    Assert.Equal("updated@example.com", update.Email);
    Assert.Equal("456", update.PhoneNumber);
    Assert.Equal("José", list.Search);
    Assert.Equal("desc", list.Order);
  }

  [Fact]
  public void Category_requests_are_normalized_idempotently()
  {
    var create = new CreateCategoryRequest { Title = " Housing ", Description = "  " };
    var update = new UpdateCategoryRequest
    {
      Title = " Utilities ",
      Description = " First\nSecond "
    };
    var list = new ListCategoryRequest { Search = " Housing ", Order = " DESC " };
    var createNormalizer = new CreateCategoryRequestNormalizer();
    var updateNormalizer = new UpdateCategoryRequestNormalizer();
    var listNormalizer = new ListCategoryRequestNormalizer();

    createNormalizer.Normalize(create);
    updateNormalizer.Normalize(update);
    listNormalizer.Normalize(list);
    createNormalizer.Normalize(create);
    updateNormalizer.Normalize(update);
    listNormalizer.Normalize(list);

    Assert.Equal("Housing", create.Title);
    Assert.Null(create.Description);
    Assert.Equal("Utilities", update.Title);
    Assert.Equal("First\nSecond", update.Description);
    Assert.Equal("Housing", list.Search);
    Assert.Equal("desc", list.Order);
  }

  [Fact]
  public void Transaction_requests_are_normalized_idempotently()
  {
    var create = new CreateTransactionRequest
    {
      Date = DateTime.UtcNow,
      Title = " Merchant ",
      Amount = -10
    };
    var update = new UpdateTransactionRequest
    {
      Date = DateTime.UtcNow,
      Title = " Updated ",
      Amount = -20
    };
    var list = new ListTransactionRequest { Search = " Merchant ", Order = " ASC " };
    var imports = new ListTransactionImportRequest
    {
      Search = " Import ",
      Status = " FINISHED ",
      Order = " ASC "
    };
    var createNormalizer = new CreateTransactionRequestNormalizer();
    var updateNormalizer = new UpdateTransactionRequestNormalizer();
    var listNormalizer = new ListTransactionRequestNormalizer();
    var importNormalizer = new ListTransactionImportRequestNormalizer();

    createNormalizer.Normalize(create);
    updateNormalizer.Normalize(update);
    listNormalizer.Normalize(list);
    importNormalizer.Normalize(imports);
    createNormalizer.Normalize(create);
    updateNormalizer.Normalize(update);
    listNormalizer.Normalize(list);
    importNormalizer.Normalize(imports);

    Assert.Equal("Merchant", create.Title);
    Assert.Equal("Updated", update.Title);
    Assert.Equal("Merchant", list.Search);
    Assert.Equal("asc", list.Order);
    Assert.Equal("Import", imports.Search);
    Assert.Equal("finished", imports.Status);
    Assert.Equal("asc", imports.Order);
  }

  [Fact]
  public void Dashboard_request_is_normalized_idempotently()
  {
    var request = new GetDashboardRequest { Order = " ASC " };
    var normalizer = new GetDashboardRequestNormalizer();

    normalizer.Normalize(request);
    normalizer.Normalize(request);

    Assert.Equal("asc", request.Order);
  }

  [Fact]
  public void Every_string_request_uses_auto_properties_and_has_one_registered_policy()
  {
    var normalizers = CreateNormalizers();
    var dispatcher = new RequestNormalizerDispatcher(normalizers);
    var requestTypes = typeof(CreatePersonRequest).Assembly
      .GetTypes()
      .Where(type => type.Namespace?.StartsWith("api.Requests", StringComparison.Ordinal) == true)
      .Where(type => type.GetProperties().Any(IsWritableStringProperty))
      .OrderBy(type => type.FullName)
      .ToArray();

    Assert.Equal(requestTypes, dispatcher.RegisteredTypes.OrderBy(type => type.FullName));

    foreach (var requestType in requestTypes)
    {
      foreach (var property in requestType.GetProperties().Where(IsWritableStringProperty))
      {
        var backingField = requestType.GetField(
          $"<{property.Name}>k__BackingField",
          BindingFlags.Instance | BindingFlags.NonPublic
        );

        Assert.NotNull(backingField);
        Assert.NotNull(backingField!.GetCustomAttribute<CompilerGeneratedAttribute>());
      }
    }
  }

  [Fact]
  public void Dispatcher_rejects_duplicate_policies()
  {
    Assert.Throws<InvalidOperationException>(() => new RequestNormalizerDispatcher(
      [new CreatePersonRequestNormalizer(), new CreatePersonRequestNormalizer()]
    ));
  }

  private static IRequestNormalizer[] CreateNormalizers()
  {
    return
    [
      new CreatePersonRequestNormalizer(),
      new UpdatePersonRequestNormalizer(),
      new ListPersonRequestNormalizer(),
      new CreateCategoryRequestNormalizer(),
      new UpdateCategoryRequestNormalizer(),
      new ListCategoryRequestNormalizer(),
      new CreateTransactionRequestNormalizer(),
      new UpdateTransactionRequestNormalizer(),
      new ListTransactionRequestNormalizer(),
      new AutoAssignTransactionsRequestNormalizer(),
      new ListTransactionImportRequestNormalizer(),
      new GetDashboardRequestNormalizer()
    ];
  }

  private static bool IsWritableStringProperty(PropertyInfo property)
  {
    return property.PropertyType == typeof(string) && property.SetMethod is not null;
  }
}
