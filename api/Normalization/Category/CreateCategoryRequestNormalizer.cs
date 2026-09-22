using api.Requests.Category;

namespace api.Normalization.Category;

public sealed class CreateCategoryRequestNormalizer : RequestNormalizer<CreateCategoryRequest>
{
  public override void Normalize(CreateCategoryRequest request)
  {
    request.Title = RequestTextNormalizer.NormalizeRequired(request.Title);
    request.Description = RequestTextNormalizer.NormalizeOptional(request.Description);
  }
}
