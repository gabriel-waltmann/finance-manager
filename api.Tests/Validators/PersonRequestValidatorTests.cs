using api.Normalization.Person;
using api.Requests.Person;
using api.Validators.Person;
using FluentValidation.TestHelper;

namespace api.Tests.Validators;

public class PersonRequestValidatorTests
{
  [Fact]
  public void List_rejects_invalid_unpaired_paging_search_and_order()
  {
    var request = new ListPersonRequest
    {
      Page = 0,
      Limit = 101,
      Search = new string('s', 201),
      Order = "newest"
    };

    var result = new ListPersonRequestValidator().TestValidate(request);
    var unpairedResult = new ListPersonRequestValidator().TestValidate(new ListPersonRequest
    {
      Page = 1
    });

    result.ShouldHaveValidationErrorFor(item => item.Page);
    result.ShouldHaveValidationErrorFor(item => item.Limit);
    result.ShouldHaveValidationErrorFor(item => item.Search);
    result.ShouldHaveValidationErrorFor(item => item.Order);
    Assert.Contains(unpairedResult.Errors, error => error.ErrorMessage.Contains("used together"));
  }

  [Fact]
  public void List_accepts_unpaged_and_trimmed_case_insensitive_order()
  {
    var unpagedRequest = new ListPersonRequest();
    var pagedRequest = new ListPersonRequest
    {
      Page = 1,
      Limit = 100,
      Order = " DESC "
    };
    var normalizer = new ListPersonRequestNormalizer();
    normalizer.Normalize(unpagedRequest);
    normalizer.Normalize(pagedRequest);
    var unpagedResult = new ListPersonRequestValidator().TestValidate(unpagedRequest);
    var pagedResult = new ListPersonRequestValidator().TestValidate(pagedRequest);

    unpagedResult.ShouldNotHaveAnyValidationErrors();
    pagedResult.ShouldNotHaveAnyValidationErrors();
  }

  [Fact]
  public void Create_rejects_blank_invalid_and_overlong_fields()
  {
    var request = new CreatePersonRequest
    {
      Name = " ",
      Email = "not-an-email",
      PhoneNumber = new string('1', 33)
    };

    var result = new CreatePersonRequestValidator().TestValidate(request);

    result.ShouldHaveValidationErrorFor(item => item.Name);
    result.ShouldHaveValidationErrorFor(item => item.Email);
    result.ShouldHaveValidationErrorFor(item => item.PhoneNumber);
  }

  [Fact]
  public void Update_accepts_fields_at_their_maximum_lengths()
  {
    var request = new UpdatePersonRequest
    {
      Name = new string('n', 120),
      Email = $"{new string('e', 242)}@example.com",
      PhoneNumber = new string('1', 32)
    };

    var result = new UpdatePersonRequestValidator().TestValidate(request);

    result.ShouldNotHaveAnyValidationErrors();
  }

  [Fact]
  public void Create_normalizes_before_validation_and_rejects_single_line_controls()
  {
    var normalized = new CreatePersonRequest
    {
      Name = $"  {new string('n', 120)}  ",
      Email = " USER@EXAMPLE.COM ",
      PhoneNumber = " <strong>123</strong> "
    };
    var unsafeRequest = new CreatePersonRequest
    {
      Name = "First\nLast",
      Email = "safe@example.com",
      PhoneNumber = "123"
    };

    var normalizer = new CreatePersonRequestNormalizer();
    normalizer.Normalize(normalized);
    normalizer.Normalize(unsafeRequest);

    var normalizedResult = new CreatePersonRequestValidator().TestValidate(normalized);
    var unsafeResult = new CreatePersonRequestValidator().TestValidate(unsafeRequest);

    normalizedResult.ShouldNotHaveAnyValidationErrors();
    Assert.Equal("user@example.com", normalized.Email);
    Assert.Equal("<strong>123</strong>", normalized.PhoneNumber);
    unsafeResult.ShouldHaveValidationErrorFor(item => item.Name);
  }
}
