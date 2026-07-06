namespace api.Exceptions;

public class NotFoundPersonException : Exception
{
  private const string message = "Person not found.";

  public NotFoundPersonException()
    : base(message) { }

  public NotFoundPersonException(string m)
    : base(message) { }

  public NotFoundPersonException(string m, Exception inner)
    : base(message, inner) { }
}
