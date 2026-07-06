namespace api.Exceptions;

public class NotFoundTransactionPersonException : Exception
{
  private const string message = "Transaction person not found.";

  public NotFoundTransactionPersonException()
    : base(message) { }

  public NotFoundTransactionPersonException(string m)
    : base(message) { }

  public NotFoundTransactionPersonException(string m, Exception inner)
    : base(message, inner) { }
}
