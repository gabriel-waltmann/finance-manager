using api.Requests.Category;

namespace api.Normalization.Category;

public sealed class ListCategoryRequestNormalizer : RequestNormalizer<ListCategoryRequest>
{
  public override void Normalize(ListCategoryRequest request)
  {
    request.Search = RequestTextNormalizer.NormalizeOptional(request.Search);
    request.Order = RequestTextNormalizer.NormalizeIdentifier(request.Order);
  }
}
