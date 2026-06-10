namespace api.Requests;

public class CreateTransactionRequest
{
  public required DateTime Date { get; set; }
  public required string Title { get; set; }
  public required decimal Amount { get; set; }
}