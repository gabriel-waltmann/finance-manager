using api.Models;

namespace api.Responses;

public class ListTransactionResponse
{
  public required List<Transaction> Transactions { get; set; }
}