namespace api.Responses.Transaction;

public class AutoAssignTransactionsResponse
{
  public required int MatchedCount { get; set; }
  public required int PersonChangedCount { get; set; }
  public required int CategoryChangedCount { get; set; }
}
