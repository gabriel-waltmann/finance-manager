using api.Requests.Category;
using FluentValidation;

namespace api.Validators.Category;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
  public CreateCategoryRequestValidator()
  {
    RuleFor(request => request.Title)
      .NotEmpty()
      .MaximumLength(120);

    RuleFor(request => request.Description)
      .MaximumLength(500)
      .When(request => request.Description != null);
  }
}
