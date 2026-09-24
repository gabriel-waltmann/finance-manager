namespace api.Requests.Transaction;

public class AutoAssignTransactionFilterRequest
{
  public string? Title { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public Guid? PersonId { get; set; }
  public bool Unassigned { get; set; }
  public Guid? CategoryId { get; set; }
  public bool Uncategorized { get; set; }
}

public class AutoAssignTransactionsRequest
{
  public AutoAssignTransactionFilterRequest Filter { get; set; } = new();
  public string PersonAction { get; set; } = "unchanged";
  public Guid? TargetPersonId { get; set; }
  public string CategoryAction { get; set; } = "unchanged";
  public List<Guid> TargetCategoryIds { get; set; } = [];
}
