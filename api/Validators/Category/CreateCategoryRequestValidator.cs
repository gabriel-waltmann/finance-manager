using api.Requests.Category;
using api.Validators.Common;
using FluentValidation;

namespace api.Validators.Category;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
  public CreateCategoryRequestValidator()
  {
    RuleFor(request => request.Title)
      .NotEmpty()
      .MaximumLength(120)
      .SafeSingleLineText();

    RuleFor(request => request.Description)
      .MaximumLength(500)
      .SafeOptionalMultilineText()
      .When(request => request.Description != null);
  }
}
