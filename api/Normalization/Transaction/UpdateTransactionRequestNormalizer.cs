using api.Requests.Transaction;

namespace api.Normalization.Transaction;

public sealed class UpdateTransactionRequestNormalizer : RequestNormalizer<UpdateTransactionRequest>
{
  public override void Normalize(UpdateTransactionRequest request)
  {
    request.Title = RequestTextNormalizer.NormalizeRequired(request.Title);
  }
}
