using api.Requests.Transaction;
using FluentValidation;

namespace api.Validators.Transaction;

public class AutoAssignTransactionsRequestValidator : AbstractValidator<AutoAssignTransactionsRequest>
{
  private static readonly string[] PersonActions = ["unchanged", "set", "clear"];
  private static readonly string[] CategoryActions = ["unchanged", "add", "replace", "clear"];

  public AutoAssignTransactionsRequestValidator()
  {
    RuleFor(request => request.Filter)
      .NotNull()
      .SetValidator(new AutoAssignTransactionFilterRequestValidator());

    RuleFor(request => request.PersonAction)
      .NotEmpty()
      .Must(action => PersonActions.Contains(action))
      .WithMessage("PersonAction must be unchanged, set, or clear.");

    RuleFor(request => request.CategoryAction)
      .NotEmpty()
      .Must(action => CategoryActions.Contains(action))
      .WithMessage("CategoryAction must be unchanged, add, replace, or clear.");

    RuleFor(request => request)
      .Must(request => request.PersonAction != "unchanged" || request.CategoryAction != "unchanged")
      .WithName(nameof(AutoAssignTransactionsRequest.PersonAction))
      .WithMessage("At least one assignment action is required.");

    RuleFor(request => request.TargetPersonId)
      .NotNull()
      .NotEqual(Guid.Empty)
      .When(request => request.PersonAction == "set")
      .WithMessage("TargetPersonId is required when PersonAction is set.");

    RuleFor(request => request.TargetPersonId)
      .Null()
      .When(request => request.PersonAction != "set")
      .WithMessage("TargetPersonId is only allowed when PersonAction is set.");

    RuleFor(request => request.TargetCategoryIds)
      .Cascade(CascadeMode.Stop)
      .NotNull()
      .Must(categoryIds => categoryIds is { Count: > 0 })
      .When(request => request.CategoryAction is "add" or "replace")
      .WithMessage("TargetCategoryIds is required when CategoryAction is add or replace.");

    RuleFor(request => request.TargetCategoryIds)
      .Must(categoryIds => categoryIds is { Count: 0 })
      .When(request => request.CategoryAction is "unchanged" or "clear")
      .WithMessage("TargetCategoryIds is only allowed when CategoryAction is add or replace.");

    RuleFor(request => request.TargetCategoryIds)
      .Must(categoryIds => categoryIds != null &&
        categoryIds.Count == categoryIds.Distinct().Count())
      .WithMessage("TargetCategoryIds must not contain duplicates.");

    RuleForEach(request => request.TargetCategoryIds)
      .NotEqual(Guid.Empty)
      .WithMessage("TargetCategoryIds must contain only non-empty GUIDs.");
  }
}
