using api.Normalization;
using api.Requests.Transaction;
using api.Validators.Common;
using FluentValidation;

namespace api.Validators.Transaction;

public class UpladTransactionRequestValidator : AbstractValidator<UpladTransactionRequest>
{
  private const long MaximumFileSize = 10 * 1024 * 1024;
  private const int MaximumFileNameLength = 255;

  public UpladTransactionRequestValidator()
  {
    RuleFor(request => request.File)
      .Cascade(CascadeMode.Stop)
      .NotNull()
      .Must(file => file.Length > 0)
      .WithMessage("File must not be empty.")
      .Must(file => file.Length <= MaximumFileSize)
      .WithMessage("File must not exceed 10 MB.")
      .Must(file => HasValidFileName(file.FileName))
      .WithMessage("File name must be between 1 and 255 characters and must not contain control characters.")
      .Must(file => Path.GetExtension(RequestTextNormalizer.NormalizeFileName(file.FileName))
        .Equals(".csv", StringComparison.OrdinalIgnoreCase))
      .WithMessage("File must have a .csv extension.");

    RuleFor(request => request.Category)
      .NotNull()
      .IsInEnum()
      .WithMessage("Category must be CreditCard or Extrato.");

    RuleFor(request => request.PersonId)
      .NotEqual(Guid.Empty)
      .When(request => request.PersonId.HasValue)
      .WithMessage("PersonId must be a non-empty GUID.");

    RuleFor(request => request.CategoryIds)
      .Cascade(CascadeMode.Stop)
      .NotNull()
      .Must(categoryIds => categoryIds.Count == categoryIds.Distinct().Count())
      .WithMessage("CategoryIds must not contain duplicates.");

    RuleForEach(request => request.CategoryIds)
      .NotEqual(Guid.Empty)
      .WithMessage("CategoryIds must contain only non-empty GUIDs.");
  }

  private static bool HasValidFileName(string fileName)
  {
    var normalizedInput = RequestTextNormalizer.NormalizeRequired(fileName);

    if (!RequestTextValidationExtensions.IsSafeSingleLine(normalizedInput))
    {
      return false;
    }

    var basename = RequestTextNormalizer.NormalizeFileName(normalizedInput);
    return basename.Length is > 0 and <= MaximumFileNameLength;
  }
}
