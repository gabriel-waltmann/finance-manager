namespace api.Requests.Transaction;

public class ListTransactionRequest
{
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public int Page { get; set; } = 1;
  public int Limit { get; set; } = 20;
  public string? WithDeleted { get; set; }
}
