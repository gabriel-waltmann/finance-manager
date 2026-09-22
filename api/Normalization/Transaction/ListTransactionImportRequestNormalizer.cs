using api.Requests.Transaction;

namespace api.Normalization.Transaction;

public sealed class ListTransactionImportRequestNormalizer :
  RequestNormalizer<ListTransactionImportRequest>
{
  public override void Normalize(ListTransactionImportRequest request)
  {
    request.Search = RequestTextNormalizer.NormalizeOptional(request.Search);
    request.Status = RequestTextNormalizer.NormalizeOptionalIdentifier(request.Status);
    request.Order = RequestTextNormalizer.NormalizeIdentifier(request.Order);
  }
}
