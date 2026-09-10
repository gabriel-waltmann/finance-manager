using api.Requests.Category;
using FluentValidation;

namespace api.Validators.Category;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
  public UpdateCategoryRequestValidator()
  {
    RuleFor(request => request.Title)
      .NotEmpty()
      .MaximumLength(120);

    RuleFor(request => request.Description)
      .MaximumLength(500)
      .When(request => request.Description != null);
  }
}
