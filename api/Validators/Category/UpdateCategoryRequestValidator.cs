using api.Requests.Category;
using api.Validators.Common;
using FluentValidation;

namespace api.Validators.Category;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
  public UpdateCategoryRequestValidator()
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
