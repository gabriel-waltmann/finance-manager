using api.Models.FileProcessingStatus;
using api.Requests.Transaction;
using api.Validators.Common;
using FluentValidation;

namespace api.Validators.Transaction;

public class ListTransactionImportRequestValidator : AbstractValidator<ListTransactionImportRequest>
{
  public ListTransactionImportRequestValidator()
  {
    RuleFor(request => request.Page)
      .GreaterThanOrEqualTo(1);

    RuleFor(request => request.Limit)
      .InclusiveBetween(1, 100);

    RuleFor(request => request.Order)
      .NotEmpty()
      .Must(BeValidOrder)
      .WithMessage("Order must be asc or desc.");

    RuleFor(request => request.Search)
      .MaximumLength(200)
      .SafeOptionalSingleLineText()
      .When(request => !string.IsNullOrWhiteSpace(request.Search));

    RuleFor(request => request.Status)
      .Must(BeValidStatus)
      .When(request => !string.IsNullOrWhiteSpace(request.Status))
      .WithMessage("Status must be Submitted, Processing, Finished, or Failed.");
  }

  private static bool BeValidOrder(string order)
  {
    return order is "asc" or "desc";
  }

  private static bool BeValidStatus(string? status)
  {
    return Enum.TryParse<FileProcessingStatusName>(status, true, out var parsed) &&
      Enum.IsDefined(parsed);
  }
}
