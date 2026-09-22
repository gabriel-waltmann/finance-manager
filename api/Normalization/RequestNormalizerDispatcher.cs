namespace api.Normalization;

public sealed class RequestNormalizerDispatcher
{
  private readonly IReadOnlyDictionary<Type, IRequestNormalizer> _normalizers;

  public RequestNormalizerDispatcher(IEnumerable<IRequestNormalizer> normalizers)
  {
    var registered = new Dictionary<Type, IRequestNormalizer>();

    foreach (var normalizer in normalizers)
    {
      if (!registered.TryAdd(normalizer.RequestType, normalizer))
      {
        throw new InvalidOperationException(
          $"Multiple request normalizers are registered for {normalizer.RequestType.FullName}."
        );
      }
    }

    _normalizers = registered;
    RegisteredTypes = registered.Keys.ToArray();
  }

  public IReadOnlyCollection<Type> RegisteredTypes { get; }

  public bool Normalize(object request)
  {
    if (!_normalizers.TryGetValue(request.GetType(), out var normalizer))
    {
      return false;
    }

    normalizer.Normalize(request);
    return true;
  }
}
