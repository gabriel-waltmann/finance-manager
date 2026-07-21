using api.Requests.TransactionPerson;
using FluentValidation;

namespace api.Validators.TransactionPerson;

public class UpdateTransactionPersonRequestValidator : AbstractValidator<UpdateTransactionPersonRequest>
{
  public UpdateTransactionPersonRequestValidator()
  {
    RuleFor(request => request.PersonId)
      .NotEmpty();

    RuleFor(request => request.TransactionId)
      .NotEmpty();
  }
}
