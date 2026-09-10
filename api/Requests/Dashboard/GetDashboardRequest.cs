namespace api.Requests.Dashboard;

public class GetDashboardRequest
{
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public Guid? PersonId { get; set; }
  public Guid? CategoryId { get; set; }
  public bool Uncategorized { get; set; }
  public int Page { get; set; } = 1;
  public int Limit { get; set; } = 20;
  public string Order { get; set; } = "desc";
}
