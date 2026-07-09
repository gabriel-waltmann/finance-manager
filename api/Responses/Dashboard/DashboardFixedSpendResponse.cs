namespace api.Responses.Dashboard;

public class DashboardFixedSpendResponse
{
  public required string Title { get; set; }
  public required int MonthCount { get; set; }
  public required string LastMonth { get; set; }
  public required decimal LastAmount { get; set; }
}
