using api.Requests.TransactionCategory;
using FluentValidation;

namespace api.Validators.TransactionCategory;

public class SetTransactionCategoriesRequestValidator : AbstractValidator<SetTransactionCategoriesRequest>
{
  public SetTransactionCategoriesRequestValidator()
  {
    RuleFor(request => request.CategoryIds)
      .NotNull()
      .Must(categoryIds => categoryIds == null || categoryIds.Count == categoryIds.Distinct().Count())
      .WithMessage("CategoryIds must not contain duplicates.");

    RuleForEach(request => request.CategoryIds)
      .NotEmpty();
  }
}
