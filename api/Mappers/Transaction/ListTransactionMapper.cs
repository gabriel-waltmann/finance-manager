using api.Models.Transaction;
using api.Responses.Transaction;

namespace api.Mappers.Transaction;

public class ListTransactionMapper
{
  public ListTransactionResponse MapResponse(List<TransactionModel> transactions)
  {
    return new ListTransactionResponse
    {
      Transactions = transactions
    };
  }
}