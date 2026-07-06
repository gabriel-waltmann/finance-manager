namespace api.Requests.TransactionPerson;

public class CreateTransactionPersonRequest
{
  public required Guid PersonId { get; set; }
  public required Guid TransactionId { get; set; }
}
