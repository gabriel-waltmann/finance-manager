namespace api.Responses.Transaction;

public class ListTransactionResponse
{
  public required List<GetTransactionResponse> Transactions { get; set; }
}
