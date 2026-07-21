using api.Validators.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace api.Tests.Validators;

public class ValidationProblemDetailsFactoryTests
{
  [Fact]
  public void Create_returns_field_keyed_problem_details()
  {
    var httpContext = new DefaultHttpContext();
    httpContext.Request.Path = "/transaction/not-a-guid";
    var modelState = new ModelStateDictionary();
    modelState.AddModelError("Id", "The value is not a valid GUID.");
    var actionContext = new ActionContext(
      httpContext,
      new RouteData(),
      new ActionDescriptor(),
      modelState
    );

    var result = Assert.IsType<BadRequestObjectResult>(
      ValidationProblemDetailsFactory.Create(actionContext)
    );
    var details = Assert.IsType<ValidationProblemDetails>(result.Value);

    Assert.Equal(StatusCodes.Status400BadRequest, details.Status);
    Assert.Equal("/transaction/not-a-guid", details.Instance);
    Assert.Equal("The value is not a valid GUID.", Assert.Single(details.Errors["Id"]));
    Assert.Contains("application/problem+json", result.ContentTypes);
  }
}
