namespace api.Exceptions;

public class NotFoundFileProcessingException : Exception
{
  private const string message = "File processing not found.";

  public NotFoundFileProcessingException() { }

  public NotFoundFileProcessingException(string m)
    : base(message) { }

  public NotFoundFileProcessingException(string m, Exception inner)
    : base(message, inner) { }
}
