using api.Requests.Person;
using api.Validators.Person;
using FluentValidation.TestHelper;

namespace api.Tests.Validators;

public class PersonRequestValidatorTests
{
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
