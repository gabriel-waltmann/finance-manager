using api.Requests.Person;

namespace api.Normalization.Person;

public sealed class ListPersonRequestNormalizer : RequestNormalizer<ListPersonRequest>
{
  public override void Normalize(ListPersonRequest request)
  {
    request.Search = RequestTextNormalizer.NormalizeOptional(request.Search);
    request.Order = RequestTextNormalizer.NormalizeIdentifier(request.Order);
  }
}
