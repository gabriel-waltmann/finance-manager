using api.Requests.Transaction;
using api.Validators.Common;
using FluentValidation;

namespace api.Validators.Transaction;

public class ListTransactionRequestValidator : AbstractValidator<ListTransactionRequest>
{
  public ListTransactionRequestValidator()
  {
    RuleFor(request => request.Page)
      .GreaterThanOrEqualTo(1);

    RuleFor(request => request.Limit)
      .InclusiveBetween(1, 100);

    RuleFor(request => request.Order)
      .NotEmpty()
      .Must(BeValidOrder)
      .WithMessage("Order must be asc or desc.");

    RuleFor(request => request.Search)
      .MaximumLength(200)
      .SafeOptionalSingleLineText()
      .When(request => !string.IsNullOrWhiteSpace(request.Search));

    RuleFor(request => request.PersonId)
      .NotEqual(Guid.Empty)
      .When(request => request.PersonId.HasValue)
      .WithMessage("PersonId must be a non-empty GUID.");

    RuleFor(request => request)
      .Must(request => !request.PersonId.HasValue || !request.Unassigned)
      .WithName(nameof(ListTransactionRequest.PersonId))
      .WithMessage("PersonId and unassigned cannot be used together.");

    RuleFor(request => request.CategoryId)
      .NotEqual(Guid.Empty)
      .When(request => request.CategoryId.HasValue)
      .WithMessage("CategoryId must be a non-empty GUID.");

    RuleFor(request => request)
      .Must(request => !request.CategoryId.HasValue || !request.Uncategorized)
      .WithName(nameof(ListTransactionRequest.CategoryId))
      .WithMessage("CategoryId and uncategorized cannot be used together.");

    RuleFor(request => request)
      .Must(request => !request.StartDate.HasValue ||
        !request.EndDate.HasValue ||
        request.StartDate.Value.Date <= request.EndDate.Value.Date)
      .WithName(nameof(ListTransactionRequest.StartDate))
      .WithMessage("Start date must be before or equal to end date.");
  }

  private static bool BeValidOrder(string order)
  {
    return order is "asc" or "desc";
  }
}
