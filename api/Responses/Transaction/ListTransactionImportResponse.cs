namespace api.Responses.Transaction;

public class ListTransactionImportResponse
{
  public required List<TransactionImportResponse> Imports { get; set; }
  public required int Page { get; set; }
  public required int Limit { get; set; }
  public required int Total { get; set; }
  public required int TotalPages { get; set; }
}
