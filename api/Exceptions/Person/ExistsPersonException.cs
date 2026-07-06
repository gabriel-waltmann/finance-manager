namespace api.Exceptions;

public class ExistsPersonException : Exception
{
  private const string message = "Person already exists.";

  public ExistsPersonException()
    : base(message) { }

  public ExistsPersonException(string m)
    : base(message) { }

  public ExistsPersonException(string m, Exception inner)
    : base(message, inner) { }
}
