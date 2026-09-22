using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace api.Normalization;

public sealed class NormalizeAndValidateRequestFilter(
  RequestNormalizerDispatcher normalizer,
  IOptions<ApiBehaviorOptions> apiBehaviorOptions
) : IAsyncActionFilter, IOrderedFilter
{
  public const int FilterOrder = -3000;

  private readonly RequestNormalizerDispatcher _normalizer = normalizer;
  private readonly ApiBehaviorOptions _apiBehaviorOptions = apiBehaviorOptions.Value;

  public int Order => FilterOrder;

  public async Task OnActionExecutionAsync(
    ActionExecutingContext context,
    ActionExecutionDelegate next
  )
  {
    foreach (var argument in context.ActionArguments.Values)
    {
      if (argument is null)
      {
        continue;
      }

      _normalizer.Normalize(argument);
      await Validate(argument, context);
    }

    if (!context.ModelState.IsValid)
    {
      context.Result = _apiBehaviorOptions.InvalidModelStateResponseFactory(context);
      return;
    }

    await next();
  }

  private static async Task Validate(object request, ActionExecutingContext context)
  {
    var validatorType = typeof(IValidator<>).MakeGenericType(request.GetType());
    var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator;

    if (validator is null)
    {
      return;
    }

    var validationContextType = typeof(ValidationContext<>).MakeGenericType(request.GetType());
    var validationContext = (IValidationContext)Activator.CreateInstance(
      validationContextType,
      request
    )!;
    var result = await validator.ValidateAsync(
      validationContext,
      context.HttpContext.RequestAborted
    );

    foreach (var failure in result.Errors)
    {
      context.ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
    }
  }
}
