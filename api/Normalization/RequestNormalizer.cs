namespace api.Normalization;

public abstract class RequestNormalizer<TRequest> :
  IRequestNormalizer,
  IRequestNormalizer<TRequest>
  where TRequest : class
{
  public Type RequestType => typeof(TRequest);

  public abstract void Normalize(TRequest request);

  void IRequestNormalizer.Normalize(object request)
  {
    if (request is not TRequest typedRequest)
    {
      throw new ArgumentException(
        $"Expected a request of type {typeof(TRequest).FullName}.",
        nameof(request)
      );
    }

    Normalize(typedRequest);
  }
}
