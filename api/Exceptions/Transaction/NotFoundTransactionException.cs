namespace api.Exceptions;

public class NotFoundTransactionException : Exception
{
  private const string message = "Transaction not found.";

  public NotFoundTransactionException() { }

  public NotFoundTransactionException(string m)
    : base(message) { }

  public NotFoundTransactionException(string m, Exception inner)
    : base(message, inner) { }
}
