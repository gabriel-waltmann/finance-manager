using api.Requests.Common;
using api.Requests.TransactionPerson;
using api.Validators.Common;
using api.Validators.TransactionPerson;
using FluentValidation.TestHelper;

namespace api.Tests.Validators;

public class AssignmentAndRouteValidatorTests
{
  [Fact]
  public void Route_rejects_empty_id()
  {
    var result = new RouteIdRequestValidator().TestValidate(new RouteIdRequest());

    result.ShouldHaveValidationErrorFor(item => item.Id);
  }

  [Fact]
  public void Create_assignment_rejects_empty_ids()
  {
    var request = new CreateTransactionPersonRequest
    {
      PersonId = Guid.Empty,
      TransactionId = Guid.Empty
    };

    var result = new CreateTransactionPersonRequestValidator().TestValidate(request);

    result.ShouldHaveValidationErrorFor(item => item.PersonId);
    result.ShouldHaveValidationErrorFor(item => item.TransactionId);
  }

  [Fact]
  public void Update_assignment_accepts_non_empty_ids()
  {
    var request = new UpdateTransactionPersonRequest
    {
      PersonId = Guid.NewGuid(),
      TransactionId = Guid.NewGuid()
    };

    var result = new UpdateTransactionPersonRequestValidator().TestValidate(request);

    result.ShouldNotHaveAnyValidationErrors();
  }
}
