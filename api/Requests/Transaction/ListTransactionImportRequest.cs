namespace api.Requests.Transaction;

public class ListTransactionImportRequest
{
  public string? Search { get; set; }
  public string? Status { get; set; }
  public int Page { get; set; } = 1;
  public int Limit { get; set; } = 20;
  public string Order { get; set; } = "desc";
}
