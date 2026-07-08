namespace api.Requests.Dashboard;

public class GetDashboardRequest
{
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public Guid? PersonId { get; set; }
  public string Order { get; set; } = "desc";
}
