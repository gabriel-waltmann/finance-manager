
using api.Models.Transaction;

namespace api.Responses.Transaction;

public class GetTransactionResponse
{
  public required TransactionModel Transaction { get; set; }
}