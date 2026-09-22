using api.Normalization.Transaction;
using api.Requests.Transaction;
using api.Validators.Transaction;
using FluentValidation.TestHelper;

namespace api.Tests.Validators;

public class TransactionRequestValidatorTests
{
  [Fact]
  public void Create_rejects_default_date_blank_title_and_zero_amount()
  {
    var request = new CreateTransactionRequest
    {
      Date = default,
      Title = " ",
      Amount = 0
    };

    var result = new CreateTransactionRequestValidator().TestValidate(request);

    result.ShouldHaveValidationErrorFor(item => item.Date);
    result.ShouldHaveValidationErrorFor(item => item.Title);
    result.ShouldHaveValidationErrorFor(item => item.Amount);
  }

  [Theory]
  [InlineData(-10.50)]
  [InlineData(10.50)]
  public void Update_accepts_positive_and_negative_amounts(double amount)
  {
    var request = new UpdateTransactionRequest
    {
      Date = DateTime.UtcNow,
      Title = new string('t', 200),
      Amount = (decimal)amount
    };

    var result = new UpdateTransactionRequestValidator().TestValidate(request);

    result.ShouldNotHaveAnyValidationErrors();
  }

  [Fact]
  public void Imported_title_is_normalized_and_rejects_control_characters()
  {
    var normalized = new CreateTransactionRequest
    {
      Date = DateTime.UtcNow,
      Title = $"  {new string('t', 200)}  ",
      Amount = -10
    };
    var unsafeRequest = new CreateTransactionRequest
    {
      Date = DateTime.UtcNow,
      Title = "Merchant\nName",
      Amount = -10
    };

    var normalizer = new CreateTransactionRequestNormalizer();
    normalizer.Normalize(normalized);
    normalizer.Normalize(unsafeRequest);

    var normalizedResult = new CreateTransactionRequestValidator().TestValidate(normalized);
    var unsafeResult = new CreateTransactionRequestValidator().TestValidate(unsafeRequest);

    normalizedResult.ShouldNotHaveAnyValidationErrors();
    Assert.Equal(200, normalized.Title.Length);
    unsafeResult.ShouldHaveValidationErrorFor(item => item.Title);
  }
}
