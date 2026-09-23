using api.Normalization.Transaction;
using api.Requests.Transaction;
using api.Validators.Transaction;
using FluentValidation.TestHelper;

namespace api.Tests.Validators;

public class AutoAssignTransactionsRequestValidatorTests
{
  [Fact]
  public void Rejects_empty_filters_and_unchanged_actions()
  {
    var request = new AutoAssignTransactionsRequest();

    var result = new AutoAssignTransactionsRequestValidator().TestValidate(request);

    Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("filter"));
    Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("assignment action"));
  }

  [Fact]
  public void Rejects_conflicting_filters_and_inverted_dates()
  {
    var request = new AutoAssignTransactionsRequest
    {
      Filter = new AutoAssignTransactionFilterRequest
      {
        StartDate = new DateTime(2026, 9, 2),
        EndDate = new DateTime(2026, 9, 1),
        PersonId = Guid.NewGuid(),
        Unassigned = true,
        CategoryId = Guid.NewGuid(),
        Uncategorized = true
      },
      PersonAction = "clear"
    };

    var result = new AutoAssignTransactionsRequestValidator().TestValidate(request);

    Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("Start date"));
    Assert.Equal(2, result.Errors.Count(error => error.ErrorMessage.Contains("cannot be used together")));
  }

  [Fact]
  public void Rejects_inconsistent_assignment_payloads()
  {
    var duplicateCategoryId = Guid.NewGuid();
    var request = new AutoAssignTransactionsRequest
    {
      Filter = new AutoAssignTransactionFilterRequest { Unassigned = true },
      PersonAction = "clear",
      TargetPersonId = Guid.NewGuid(),
      CategoryAction = "add",
      TargetCategoryIds = [Guid.Empty, duplicateCategoryId, duplicateCategoryId]
    };

    var result = new AutoAssignTransactionsRequestValidator().TestValidate(request);

    result.ShouldHaveValidationErrorFor(item => item.TargetPersonId);
    result.ShouldHaveValidationErrorFor(item => item.TargetCategoryIds);
    Assert.Contains(result.Errors, error => error.PropertyName.Contains("TargetCategoryIds[0]"));
  }

  [Fact]
  public void Normalizes_actions_and_accepts_a_valid_request()
  {
    var request = new AutoAssignTransactionsRequest
    {
      Filter = new AutoAssignTransactionFilterRequest
      {
        StartDate = new DateTime(2026, 9, 1)
      },
      PersonAction = " SET ",
      TargetPersonId = Guid.NewGuid(),
      CategoryAction = " ADD ",
      TargetCategoryIds = [Guid.NewGuid()]
    };

    new AutoAssignTransactionsRequestNormalizer().Normalize(request);
    var result = new AutoAssignTransactionsRequestValidator().TestValidate(request);

    Assert.Equal("set", request.PersonAction);
    Assert.Equal("add", request.CategoryAction);
    result.ShouldNotHaveAnyValidationErrors();
  }
}
