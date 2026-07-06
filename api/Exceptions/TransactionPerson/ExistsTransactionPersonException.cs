namespace api.Exceptions;

public class ExistsTransactionPersonException : Exception
{
  private const string message = "Transaction person already exists.";

  public ExistsTransactionPersonException()
    : base(message) { }

  public ExistsTransactionPersonException(string m)
    : base(message) { }

  public ExistsTransactionPersonException(string m, Exception inner)
    : base(message, inner) { }
}
