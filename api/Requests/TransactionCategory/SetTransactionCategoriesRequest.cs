namespace api.Requests.TransactionCategory;

public class SetTransactionCategoriesRequest
{
  public required List<Guid> CategoryIds { get; set; }
}
