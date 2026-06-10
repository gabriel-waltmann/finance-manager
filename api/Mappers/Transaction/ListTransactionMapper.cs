using api.Models;
using api.Responses;

namespace api.Mappers;

public class ListTransactionMapper
{
  public ListTransactionResponse MapResponse(List<Transaction> transactions)
  {
    return new ListTransactionResponse
    {
      Transactions = transactions
    };
  }
}