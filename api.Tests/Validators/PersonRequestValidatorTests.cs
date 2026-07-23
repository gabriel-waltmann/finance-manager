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
    var unpagedResult = new ListPersonRequestValidator().TestValidate(new ListPersonRequest());
    var pagedResult = new ListPersonRequestValidator().TestValidate(new ListPersonRequest
    {
      Page = 1,
      Limit = 100,
      Order = " DESC "
    });

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
}
