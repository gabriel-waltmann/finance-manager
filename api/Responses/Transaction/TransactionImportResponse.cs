namespace api.Responses.Transaction;

public class TransactionImportResponse
{
  public required Guid Id { get; set; }
  public required string FileName { get; set; }
  public required string Category { get; set; }
  public required string Status { get; set; }
  public required int TransactionCount { get; set; }
  public required DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
}
