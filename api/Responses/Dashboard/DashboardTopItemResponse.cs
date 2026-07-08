namespace api.Responses.Dashboard;

public class DashboardTopItemResponse
{
  public required string Title { get; set; }
  public required decimal TotalAmount { get; set; }
  public required int TransactionCount { get; set; }
}
