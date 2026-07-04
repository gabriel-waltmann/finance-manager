namespace api.Exceptions;

public class NotFoundFileException : Exception
{
  private const string message = "File not found.";

  public NotFoundFileException() { }

  public NotFoundFileException(string m)
    : base(message) { }

  public NotFoundFileException(string m, Exception inner)
    : base(message, inner) { }
}
