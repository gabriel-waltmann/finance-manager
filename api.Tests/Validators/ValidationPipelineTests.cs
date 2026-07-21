using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using api.Requests.Common;
using api.Requests.Person;
using api.Requests.Transaction;
using api.Validators;
using api.Validators.Common;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace api.Tests.Validators;

public class ValidationPipelineTests
{
  [Theory]
  [InlineData("POST", "/_test/validation/body")]
  [InlineData("GET", "/_test/validation/query?page=0")]
  [InlineData("GET", "/_test/validation/route/not-a-guid")]
  [InlineData("POST", "/_test/validation/form")]
  public async Task Invalid_inputs_return_problem_details_before_action_execution(
    string method,
    string path
  )
  {
    await using var app = await CreateApp();
    TestValidationController.ExecutionCount = 0;
    using var request = new HttpRequestMessage(new HttpMethod(method), path);

    if (path.EndsWith("/body", StringComparison.Ordinal))
    {
      request.Content = JsonContent.Create(new { });
    }
    else if (path.EndsWith("/form", StringComparison.Ordinal))
    {
      request.Content = new MultipartFormDataContent();
    }

    using var response = await app.GetTestClient().SendAsync(request);
    var json = await response.Content.ReadFromJsonAsync<JsonElement>();

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    Assert.True(json.TryGetProperty("errors", out var errors));
    Assert.NotEmpty(errors.EnumerateObject().ToList());
    Assert.Equal(0, TestValidationController.ExecutionCount);
  }

  private static async Task<WebApplication> CreateApp()
  {
    var builder = WebApplication.CreateBuilder();
    builder.WebHost.UseTestServer();
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<ValidatorAssemblyMarker>();
    builder.Services.Configure<ApiBehaviorOptions>(options =>
      options.InvalidModelStateResponseFactory = ValidationProblemDetailsFactory.Create
    );
    builder.Services
      .AddControllers()
      .AddApplicationPart(typeof(TestValidationController).Assembly);

    var app = builder.Build();
    app.MapControllers();
    await app.StartAsync();
    return app;
  }
}

[ApiController]
[Route("/_test/validation")]
public class TestValidationController : ControllerBase
{
  public static int ExecutionCount { get; set; }

  [HttpPost("body")]
  public IActionResult Body([FromBody] CreatePersonRequest request)
  {
    ExecutionCount++;
    return NoContent();
  }

  [HttpGet("query")]
  public IActionResult Query([FromQuery] ListTransactionRequest request)
  {
    ExecutionCount++;
    return NoContent();
  }

  [HttpGet("route/{id}")]
  public IActionResult Route([FromRoute] RouteIdRequest request)
  {
    ExecutionCount++;
    return NoContent();
  }

  [HttpPost("form")]
  public IActionResult Form([FromForm] UpladTransactionRequest request)
  {
    ExecutionCount++;
    return NoContent();
  }
}
