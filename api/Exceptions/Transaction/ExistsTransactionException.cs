namespace api.Exceptions;

public class ExistsTransactionException : Exception
{
  private const string message = "Transaction already exists.";

  public ExistsTransactionException()
    : base(message) { }

  public ExistsTransactionException(string m)
    : base(message) { }

  public ExistsTransactionException(string m, Exception inner)
    : base(message, inner) { }
}
