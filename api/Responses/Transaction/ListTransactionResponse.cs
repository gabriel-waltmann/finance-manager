namespace api.Responses.Transaction;

public class ListTransactionResponse
{
  public required List<GetTransactionResponse> Transactions { get; set; }
  public required int Page { get; set; }
  public required int Limit { get; set; }
  public required int Total { get; set; }
  public required int TotalPages { get; set; }
}
