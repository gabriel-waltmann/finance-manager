using System.Globalization;
using FluentValidation;

namespace api.Validators.Common;

public static class RequestTextValidationExtensions
{
  public static IRuleBuilderOptions<T, string> SafeSingleLineText<T>(
    this IRuleBuilder<T, string> ruleBuilder
  )
  {
    return ruleBuilder
      .Must(IsSafeSingleLine)
      .WithMessage("'{PropertyName}' must not contain control characters.");
  }

  public static IRuleBuilderOptions<T, string?> SafeOptionalSingleLineText<T>(
    this IRuleBuilder<T, string?> ruleBuilder
  )
  {
    return ruleBuilder
      .Must(value => value is null || IsSafeSingleLine(value))
      .WithMessage("'{PropertyName}' must not contain control characters.");
  }

  public static IRuleBuilderOptions<T, string?> SafeOptionalMultilineText<T>(
    this IRuleBuilder<T, string?> ruleBuilder
  )
  {
    return ruleBuilder
      .Must(value => value is null || IsSafeMultiline(value))
      .WithMessage("'{PropertyName}' contains an unsupported control character.");
  }

  public static bool IsSafeSingleLine(string value)
  {
    return value.All(character => char.GetUnicodeCategory(character) is not (
      UnicodeCategory.Control or
      UnicodeCategory.LineSeparator or
      UnicodeCategory.ParagraphSeparator
    ));
  }

  public static bool IsSafeMultiline(string value)
  {
    return value.All(character =>
      !char.IsControl(character) || character is '\t' or '\r' or '\n'
    );
  }
}
