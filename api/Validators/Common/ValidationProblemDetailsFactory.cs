using Microsoft.AspNetCore.Mvc;

namespace api.Validators.Common;

public static class ValidationProblemDetailsFactory
{
  public static IActionResult Create(ActionContext context)
  {
    var problemDetails = new ValidationProblemDetails(context.ModelState)
    {
      Status = StatusCodes.Status400BadRequest,
      Title = "One or more validation errors occurred.",
      Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
      Instance = context.HttpContext.Request.Path
    };

    return new BadRequestObjectResult(problemDetails)
    {
      ContentTypes = { "application/problem+json" }
    };
  }
}
