using api.Requests.Person;

namespace api.Normalization.Person;

public sealed class CreatePersonRequestNormalizer : RequestNormalizer<CreatePersonRequest>
{
  public override void Normalize(CreatePersonRequest request)
  {
    request.Name = RequestTextNormalizer.NormalizeRequired(request.Name);
    request.Email = RequestTextNormalizer.NormalizeIdentifier(request.Email);
    request.PhoneNumber = RequestTextNormalizer.NormalizeRequired(request.PhoneNumber);
  }
}
