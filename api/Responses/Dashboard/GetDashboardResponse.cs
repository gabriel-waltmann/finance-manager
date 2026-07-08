namespace api.Responses.Dashboard;

public class GetDashboardResponse
{
  public required List<DashboardTopItemResponse> TopItems { get; set; }
  public required decimal TotalAmount { get; set; }
}
