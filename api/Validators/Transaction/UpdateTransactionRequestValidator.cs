using api.Requests.Transaction;
using api.Validators.Common;
using FluentValidation;

namespace api.Validators.Transaction;

public class UpdateTransactionRequestValidator : AbstractValidator<UpdateTransactionRequest>
{
  public UpdateTransactionRequestValidator()
  {
    RuleFor(request => request.Date)
      .NotEmpty();

    RuleFor(request => request.Title)
      .NotEmpty()
      .MaximumLength(200)
      .SafeSingleLineText();

    RuleFor(request => request.Amount)
      .NotEqual(0);
  }
}
