using System.Text;

namespace api.Normalization;

public static class RequestTextNormalizer
{
  public static string NormalizeRequired(string? value)
  {
    return value is null
      ? null!
      : value.Normalize(NormalizationForm.FormC).Trim();
  }

  public static string? NormalizeOptional(string? value)
  {
    var normalized = NormalizeRequired(value);
    return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
  }

  public static string NormalizeIdentifier(string? value)
  {
    return NormalizeRequired(value)?.ToLowerInvariant()!;
  }

  public static string? NormalizeOptionalIdentifier(string? value)
  {
    return NormalizeOptional(value)?.ToLowerInvariant();
  }

  public static string NormalizeFileName(string? value)
  {
    var normalized = NormalizeRequired(value).Replace('\\', '/');
    return NormalizeRequired(Path.GetFileName(normalized));
  }
}
