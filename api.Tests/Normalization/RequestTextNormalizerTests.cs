using api.Normalization;
using api.Validators.Common;

namespace api.Tests.Normalization;

public class RequestTextNormalizerTests
{
  [Fact]
  public void Normalization_applies_nfc_trimming_optional_nulls_and_identifier_casing()
  {
    Assert.Equal("Café", RequestTextNormalizer.NormalizeRequired("  Cafe\u0301  "));
    Assert.Null(RequestTextNormalizer.NormalizeOptional(" \t\r\n "));
    Assert.Equal("user@example.com", RequestTextNormalizer.NormalizeIdentifier(" USER@EXAMPLE.COM "));
  }

  [Fact]
  public void File_name_normalization_accepts_both_path_separator_styles()
  {
    Assert.Equal("Café.csv", RequestTextNormalizer.NormalizeFileName(" C:\\fakepath\\Cafe\u0301.csv "));
    Assert.Equal("transactions.csv", RequestTextNormalizer.NormalizeFileName("/tmp/transactions.csv"));
  }

  [Fact]
  public void Text_safety_distinguishes_single_and_multiline_fields()
  {
    Assert.False(RequestTextValidationExtensions.IsSafeSingleLine("first\nsecond"));
    Assert.False(RequestTextValidationExtensions.IsSafeSingleLine("first\u2028second"));
    Assert.True(RequestTextValidationExtensions.IsSafeMultiline("first\tsecond\r\nthird"));
    Assert.False(RequestTextValidationExtensions.IsSafeMultiline("unsafe\0text"));
  }
}
