using api.Requests.Transaction;

namespace api.Normalization.Transaction;

public sealed class CreateTransactionRequestNormalizer : RequestNormalizer<CreateTransactionRequest>
{
  public override void Normalize(CreateTransactionRequest request)
  {
    request.Title = RequestTextNormalizer.NormalizeRequired(request.Title);
  }
}
