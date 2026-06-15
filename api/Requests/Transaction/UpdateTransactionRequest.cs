namespace api.Requests.Transaction;

public class UpdateTransactionRequest
{
  public required DateTime Date { get; set; }
  public required string Title { get; set; }
  public required decimal Amount { get; set; }
}