using api.Requests.Person;
using FluentValidation;

namespace api.Validators.Person;

public class UpdatePersonRequestValidator : AbstractValidator<UpdatePersonRequest>
{
  public UpdatePersonRequestValidator()
  {
    RuleFor(request => request.Name)
      .NotEmpty()
      .MaximumLength(120);

    RuleFor(request => request.Email)
      .NotEmpty()
      .MaximumLength(254)
      .EmailAddress();

    RuleFor(request => request.PhoneNumber)
      .NotEmpty()
      .MaximumLength(32);
  }
}
