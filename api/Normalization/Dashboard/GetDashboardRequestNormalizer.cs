using api.Requests.Dashboard;

namespace api.Normalization.Dashboard;

public sealed class GetDashboardRequestNormalizer : RequestNormalizer<GetDashboardRequest>
{
  public override void Normalize(GetDashboardRequest request)
  {
    request.Order = RequestTextNormalizer.NormalizeIdentifier(request.Order);
  }
}
