using api.Models.FileCategory;
using api.Requests.Transaction;
using api.Validators.Transaction;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;

namespace api.Tests.Validators;

public class UploadTransactionRequestValidatorTests
{
  [Fact]
  public void Upload_rejects_empty_non_csv_file_and_missing_category()
  {
    var request = new UpladTransactionRequest
    {
      File = CreateFile(Array.Empty<byte>(), "transactions.txt"),
      Category = null
    };

    var result = new UpladTransactionRequestValidator().TestValidate(request);

    result.ShouldHaveValidationErrorFor(item => item.File);
    result.ShouldHaveValidationErrorFor(item => item.Category);
  }

  [Fact]
  public void Upload_accepts_csv_at_ten_megabytes()
  {
    var request = new UpladTransactionRequest
    {
      File = CreateFile(new byte[10 * 1024 * 1024], "transactions.CSV"),
      Category = FileCategoryName.CreditCard
    };

    var result = new UpladTransactionRequestValidator().TestValidate(request);

    result.ShouldNotHaveAnyValidationErrors();
  }

  [Fact]
  public void Upload_rejects_file_over_ten_megabytes()
  {
    var request = new UpladTransactionRequest
    {
      File = CreateFile(new byte[(10 * 1024 * 1024) + 1], "transactions.csv"),
      Category = FileCategoryName.Extrato
    };

    var result = new UpladTransactionRequestValidator().TestValidate(request);

    result.ShouldHaveValidationErrorFor(item => item.File);
  }

  [Fact]
  public void Upload_accepts_optional_person_and_multiple_categories()
  {
    var request = new UpladTransactionRequest
    {
      File = CreateFile([1], "transactions.csv"),
      Category = FileCategoryName.Extrato,
      PersonId = Guid.NewGuid(),
      CategoryIds = [Guid.NewGuid(), Guid.NewGuid()]
    };

    var result = new UpladTransactionRequestValidator().TestValidate(request);

    result.ShouldNotHaveAnyValidationErrors();
  }

  [Fact]
  public void Upload_rejects_empty_assignment_ids_and_duplicate_categories()
  {
    var duplicateCategoryId = Guid.NewGuid();
    var request = new UpladTransactionRequest
    {
      File = CreateFile([1], "transactions.csv"),
      Category = FileCategoryName.CreditCard,
      PersonId = Guid.Empty,
      CategoryIds = [Guid.Empty, duplicateCategoryId, duplicateCategoryId]
    };

    var result = new UpladTransactionRequestValidator().TestValidate(request);

    result.ShouldHaveValidationErrorFor(item => item.PersonId);
    result.ShouldHaveValidationErrorFor(item => item.CategoryIds);
    result.ShouldHaveValidationErrorFor("CategoryIds[0]");
  }

  [Fact]
  public void Upload_rejects_unsafe_or_overlong_file_basenames()
  {
    var unsafeRequest = new UpladTransactionRequest
    {
      File = CreateFile([1], "unsafe\nname.csv"),
      Category = FileCategoryName.Extrato
    };
    var overlongRequest = new UpladTransactionRequest
    {
      File = CreateFile([1], $"{new string('f', 252)}.csv"),
      Category = FileCategoryName.Extrato
    };

    new UpladTransactionRequestValidator()
      .TestValidate(unsafeRequest)
      .ShouldHaveValidationErrorFor(item => item.File);
    new UpladTransactionRequestValidator()
      .TestValidate(overlongRequest)
      .ShouldHaveValidationErrorFor(item => item.File);
  }

  private static FormFile CreateFile(byte[] data, string fileName)
  {
    return new FormFile(new MemoryStream(data), 0, data.Length, "File", fileName);
  }
}
