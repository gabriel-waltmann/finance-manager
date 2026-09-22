using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using api.Requests.Common;
using api.Requests.Person;
using api.Requests.Transaction;
using api.Normalization;
using api.Validators;
using api.Validators.Common;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace api.Tests.Validators;

public class ValidationPipelineTests
{
  [Fact]
  public async Task Bound_body_and_query_values_are_normalized_before_action_execution()
  {
    await using var app = await CreateApp();
    TestValidationController.LastBody = null;
    TestValidationController.LastQuery = null;

    using var bodyResponse = await app.GetTestClient().PostAsJsonAsync(
      "/_test/validation/body",
      new
      {
        Name = "  Cafe\u0301  ",
        Email = " USER@EXAMPLE.COM ",
        PhoneNumber = " 123 "
      }
    );
    using var queryResponse = await app.GetTestClient().GetAsync(
      "/_test/validation/query?search=%20Cafe%CC%81%20&order=%20ASC%20"
    );

    Assert.Equal(HttpStatusCode.NoContent, bodyResponse.StatusCode);
    Assert.Equal(HttpStatusCode.NoContent, queryResponse.StatusCode);
    Assert.Equal("Café", TestValidationController.LastBody?.Name);
    Assert.Equal("user@example.com", TestValidationController.LastBody?.Email);
    Assert.Equal("123", TestValidationController.LastBody?.PhoneNumber);
    Assert.Equal("Café", TestValidationController.LastQuery?.Search);
    Assert.Equal("asc", TestValidationController.LastQuery?.Order);
  }

  [Fact]
  public async Task Control_characters_return_problem_details_before_action_execution()
  {
    await using var app = await CreateApp();
    TestValidationController.ExecutionCount = 0;

    using var response = await app.GetTestClient().PostAsJsonAsync(
      "/_test/validation/body",
      new
      {
        Name = "First\nLast",
        Email = "user@example.com",
        PhoneNumber = "123"
      }
    );
    var json = await response.Content.ReadFromJsonAsync<JsonElement>();

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    Assert.True(json.GetProperty("errors").TryGetProperty("Name", out _));
    Assert.Equal(0, TestValidationController.ExecutionCount);
  }

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
    builder.Services.AddValidatorsFromAssemblyContaining<ValidatorAssemblyMarker>();
    builder.Services.Configure<ApiBehaviorOptions>(options =>
      options.InvalidModelStateResponseFactory = ValidationProblemDetailsFactory.Create
    );
    builder.Services
      .AddControllers()
      .AddApplicationPart(typeof(TestValidationController).Assembly)
      .AddRequestNormalization();

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
  public static CreatePersonRequest? LastBody { get; set; }
  public static ListTransactionRequest? LastQuery { get; set; }

  [HttpPost("body")]
  public IActionResult Body([FromBody] CreatePersonRequest request)
  {
    ExecutionCount++;
    LastBody = request;
    return NoContent();
  }

  [HttpGet("query")]
  public IActionResult Query([FromQuery] ListTransactionRequest request)
  {
    ExecutionCount++;
    LastQuery = request;
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
