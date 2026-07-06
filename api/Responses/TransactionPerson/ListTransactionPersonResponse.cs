using api.Models.TransactionPerson;

namespace api.Responses.TransactionPerson;

public class ListTransactionPersonResponse
{
  public required List<TransactionPersonModel> TransactionPersons { get; set; }
}
