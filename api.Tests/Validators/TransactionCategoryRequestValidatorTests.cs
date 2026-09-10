using api.Requests.TransactionCategory;
using api.Validators.TransactionCategory;
using FluentValidation.TestHelper;

namespace api.Tests.Validators;

public class TransactionCategoryRequestValidatorTests
{
  [Fact]
  public void Set_rejects_empty_and_duplicate_category_ids()
  {
    var categoryId = Guid.NewGuid();
    var request = new SetTransactionCategoriesRequest
    {
      CategoryIds = [Guid.Empty, categoryId, categoryId]
    };

    var result = new SetTransactionCategoriesRequestValidator().TestValidate(request);

    Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("duplicates"));
    Assert.Contains(result.Errors, error => error.PropertyName.Contains("CategoryIds[0]"));
  }

  [Fact]
  public void Set_accepts_empty_and_distinct_category_sets()
  {
    var emptyResult = new SetTransactionCategoriesRequestValidator().TestValidate(
      new SetTransactionCategoriesRequest { CategoryIds = [] }
    );
    var distinctResult = new SetTransactionCategoriesRequestValidator().TestValidate(
      new SetTransactionCategoriesRequest { CategoryIds = [Guid.NewGuid(), Guid.NewGuid()] }
    );

    emptyResult.ShouldNotHaveAnyValidationErrors();
    distinctResult.ShouldNotHaveAnyValidationErrors();
  }
}
