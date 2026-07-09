namespace api.Responses.Dashboard;

public class GetDashboardResponse
{
  public required List<DashboardTopItemResponse> TopItems { get; set; }
  public required List<DashboardFixedSpendResponse> FixedSpends { get; set; }
  public required decimal TotalAmount { get; set; }
  public required int Page { get; set; }
  public required int Limit { get; set; }
  public required int Total { get; set; }
  public required int TotalPages { get; set; }
}
