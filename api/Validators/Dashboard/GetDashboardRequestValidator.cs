using api.Requests.Dashboard;
using FluentValidation;

namespace api.Validators.Dashboard;

public class GetDashboardRequestValidator : AbstractValidator<GetDashboardRequest>
{
  public GetDashboardRequestValidator()
  {
    RuleFor(request => request.Page)
      .GreaterThanOrEqualTo(1);

    RuleFor(request => request.Limit)
      .InclusiveBetween(1, 100);

    RuleFor(request => request.Order)
      .NotEmpty()
      .Must(BeValidOrder)
      .WithMessage("Order must be asc or desc.");

    RuleFor(request => request.PersonId)
      .NotEqual(Guid.Empty)
      .When(request => request.PersonId.HasValue)
      .WithMessage("PersonId must be a non-empty GUID.");

    RuleFor(request => request)
      .Must(request => !request.StartDate.HasValue ||
        !request.EndDate.HasValue ||
        request.StartDate.Value.Date <= request.EndDate.Value.Date)
      .WithName(nameof(GetDashboardRequest.StartDate))
      .WithMessage("Start date must be before or equal to end date.");
  }

  private static bool BeValidOrder(string order)
  {
    return order.Trim().Equals("asc", StringComparison.OrdinalIgnoreCase) ||
      order.Trim().Equals("desc", StringComparison.OrdinalIgnoreCase);
  }
}
