using api.Requests.Transaction;
using FluentValidation;

namespace api.Validators.Transaction;

public class AutoAssignTransactionFilterRequestValidator :
  AbstractValidator<AutoAssignTransactionFilterRequest>
{
  public AutoAssignTransactionFilterRequestValidator()
  {
    RuleFor(request => request.PersonId)
      .NotEqual(Guid.Empty)
      .When(request => request.PersonId.HasValue)
      .WithMessage("PersonId must be a non-empty GUID.");

    RuleFor(request => request)
      .Must(request => !request.PersonId.HasValue || !request.Unassigned)
      .WithName(nameof(AutoAssignTransactionFilterRequest.PersonId))
      .WithMessage("PersonId and unassigned cannot be used together.");

    RuleFor(request => request.CategoryId)
      .NotEqual(Guid.Empty)
      .When(request => request.CategoryId.HasValue)
      .WithMessage("CategoryId must be a non-empty GUID.");

    RuleFor(request => request)
      .Must(request => !request.CategoryId.HasValue || !request.Uncategorized)
      .WithName(nameof(AutoAssignTransactionFilterRequest.CategoryId))
      .WithMessage("CategoryId and uncategorized cannot be used together.");

    RuleFor(request => request)
      .Must(request => !request.StartDate.HasValue ||
        !request.EndDate.HasValue ||
        request.StartDate.Value.Date <= request.EndDate.Value.Date)
      .WithName(nameof(AutoAssignTransactionFilterRequest.StartDate))
      .WithMessage("Start date must be before or equal to end date.");

    RuleFor(request => request)
      .Must(HasAnyFilter)
      .WithName(nameof(AutoAssignTransactionsRequest.Filter))
      .WithMessage("At least one transaction filter is required.");
  }

  private static bool HasAnyFilter(AutoAssignTransactionFilterRequest request)
  {
    return request.StartDate.HasValue ||
      request.EndDate.HasValue ||
      request.PersonId.HasValue ||
      request.Unassigned ||
      request.CategoryId.HasValue ||
      request.Uncategorized;
  }
}
