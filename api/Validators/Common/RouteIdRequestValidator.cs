using api.Requests.Common;
using FluentValidation;

namespace api.Validators.Common;

public class RouteIdRequestValidator : AbstractValidator<RouteIdRequest>
{
  public RouteIdRequestValidator()
  {
    RuleFor(request => request.Id)
      .NotEmpty()
      .WithMessage("Id must be a non-empty GUID.");
  }
}
