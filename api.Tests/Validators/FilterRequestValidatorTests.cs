using api.Requests.Dashboard;
using api.Requests.Transaction;
using api.Validators.Dashboard;
using api.Validators.Transaction;
using FluentValidation.TestHelper;

namespace api.Tests.Validators;

public class FilterRequestValidatorTests
{
  [Fact]
  public void Transaction_list_rejects_invalid_paging_filters_and_dates()
  {
    var request = new ListTransactionRequest
    {
      Page = 0,
      Limit = 101,
      Order = "newest",
      Search = new string('s', 201),
      PersonId = Guid.NewGuid(),
      Unassigned = true,
      StartDate = new DateTime(2026, 2, 2),
      EndDate = new DateTime(2026, 2, 1)
    };

    var result = new ListTransactionRequestValidator().TestValidate(request);

    result.ShouldHaveValidationErrorFor(item => item.Page);
    result.ShouldHaveValidationErrorFor(item => item.Limit);
    result.ShouldHaveValidationErrorFor(item => item.Order);
    result.ShouldHaveValidationErrorFor(item => item.Search);
    Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("cannot be used together"));
    Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("Start date"));
  }

  [Fact]
  public void Dashboard_accepts_trimmed_case_insensitive_order_and_equal_dates()
  {
    var date = new DateTime(2026, 2, 1);
    var request = new GetDashboardRequest
    {
      Page = 1,
      Limit = 100,
      Order = " ASC ",
      StartDate = date,
      EndDate = date
    };

    var result = new GetDashboardRequestValidator().TestValidate(request);

    result.ShouldNotHaveAnyValidationErrors();
  }

  [Fact]
  public void Import_list_rejects_unknown_status()
  {
    var request = new ListTransactionImportRequest { Status = "Queued" };

    var result = new ListTransactionImportRequestValidator().TestValidate(request);

    result.ShouldHaveValidationErrorFor(item => item.Status);
  }
}
