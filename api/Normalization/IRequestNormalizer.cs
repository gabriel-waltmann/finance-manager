namespace api.Normalization;

public interface IRequestNormalizer
{
  Type RequestType { get; }

  void Normalize(object request);
}

public interface IRequestNormalizer<in TRequest>
  where TRequest : class
{
  void Normalize(TRequest request);
}
