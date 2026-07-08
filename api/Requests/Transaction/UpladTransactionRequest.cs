namespace api.Requests.Transaction;

using api.Models.FileCategory;

public class UpladTransactionRequest
{
  public required IFormFile File { get; set; }
  public FileCategoryName? Category { get; set; }
}
