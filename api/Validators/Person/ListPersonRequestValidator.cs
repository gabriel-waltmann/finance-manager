using api.Requests.Person;
using FluentValidation;

namespace api.Validators.Person;

public class ListPersonRequestValidator : AbstractValidator<ListPersonRequest>
{
  public ListPersonRequestValidator()
  {
    RuleFor(request => request.Page)
      .GreaterThanOrEqualTo(1)
      .When(request => request.Page.HasValue);

    RuleFor(request => request.Limit)
      .InclusiveBetween(1, 100)
      .When(request => request.Limit.HasValue);

    RuleFor(request => request)
      .Must(request => request.Page.HasValue == request.Limit.HasValue)
      .WithName(nameof(ListPersonRequest.Page))
      .WithMessage("Page and limit must be used together.");

    RuleFor(request => request.Order)
      .NotEmpty()
      .Must(BeValidOrder)
      .WithMessage("Order must be asc or desc.");

    RuleFor(request => request.Search)
      .MaximumLength(200)
      .When(request => !string.IsNullOrWhiteSpace(request.Search));
  }

  private static bool BeValidOrder(string order)
  {
    return order.Trim().Equals("asc", StringComparison.OrdinalIgnoreCase) ||
      order.Trim().Equals("desc", StringComparison.OrdinalIgnoreCase);
  }
}
