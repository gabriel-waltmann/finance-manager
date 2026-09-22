using api.Requests.Person;

namespace api.Normalization.Person;

public sealed class UpdatePersonRequestNormalizer : RequestNormalizer<UpdatePersonRequest>
{
  public override void Normalize(UpdatePersonRequest request)
  {
    request.Name = RequestTextNormalizer.NormalizeRequired(request.Name);
    request.Email = RequestTextNormalizer.NormalizeIdentifier(request.Email);
    request.PhoneNumber = RequestTextNormalizer.NormalizeRequired(request.PhoneNumber);
  }
}
