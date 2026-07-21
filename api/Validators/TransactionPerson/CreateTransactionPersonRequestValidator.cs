using api.Requests.TransactionPerson;
using FluentValidation;

namespace api.Validators.TransactionPerson;

public class CreateTransactionPersonRequestValidator : AbstractValidator<CreateTransactionPersonRequest>
{
  public CreateTransactionPersonRequestValidator()
  {
    RuleFor(request => request.PersonId)
      .NotEmpty();

    RuleFor(request => request.TransactionId)
      .NotEmpty();
  }
}
