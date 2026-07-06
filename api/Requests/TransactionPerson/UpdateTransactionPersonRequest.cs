namespace api.Requests.TransactionPerson;

public class UpdateTransactionPersonRequest
{
  public required Guid PersonId { get; set; }
  public required Guid TransactionId { get; set; }
}
