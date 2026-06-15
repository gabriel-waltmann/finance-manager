using api.Models.Transaction;

namespace api.Responses.Transaction;

public class ListTransactionResponse
{
  public required List<TransactionModel> Transactions { get; set; }
}