using api.Requests.Category;

namespace api.Normalization.Category;

public sealed class UpdateCategoryRequestNormalizer : RequestNormalizer<UpdateCategoryRequest>
{
  public override void Normalize(UpdateCategoryRequest request)
  {
    request.Title = RequestTextNormalizer.NormalizeRequired(request.Title);
    request.Description = RequestTextNormalizer.NormalizeOptional(request.Description);
  }
}
