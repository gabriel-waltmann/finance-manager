using api.Requests.Transaction;

namespace api.Normalization.Transaction;

public sealed class AutoAssignTransactionsRequestNormalizer :
  RequestNormalizer<AutoAssignTransactionsRequest>
{
  public override void Normalize(AutoAssignTransactionsRequest request)
  {
    request.PersonAction = RequestTextNormalizer.NormalizeIdentifier(request.PersonAction);
    request.CategoryAction = RequestTextNormalizer.NormalizeIdentifier(request.CategoryAction);
  }
}
