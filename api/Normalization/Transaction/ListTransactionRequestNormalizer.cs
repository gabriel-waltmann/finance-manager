using api.Requests.Transaction;

namespace api.Normalization.Transaction;

public sealed class ListTransactionRequestNormalizer : RequestNormalizer<ListTransactionRequest>
{
  public override void Normalize(ListTransactionRequest request)
  {
    request.Search = RequestTextNormalizer.NormalizeOptional(request.Search);
    request.Title = RequestTextNormalizer.NormalizeOptional(request.Title);
    request.Order = RequestTextNormalizer.NormalizeIdentifier(request.Order);
  }
}
