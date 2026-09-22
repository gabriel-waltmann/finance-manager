using api.Requests.Person;
using api.Validators.Common;
using FluentValidation;

namespace api.Validators.Person;

public class UpdatePersonRequestValidator : AbstractValidator<UpdatePersonRequest>
{
  public UpdatePersonRequestValidator()
  {
    RuleFor(request => request.Name)
      .NotEmpty()
      .MaximumLength(120)
      .SafeSingleLineText();

    RuleFor(request => request.Email)
      .NotEmpty()
      .MaximumLength(254)
      .EmailAddress()
      .SafeSingleLineText();

    RuleFor(request => request.PhoneNumber)
      .NotEmpty()
      .MaximumLength(32)
      .SafeSingleLineText();
  }
}
