namespace api.Exceptions;

public class ExistsTransactionCategoryException : Exception
{
  private const string message = "Transaction category already exists.";

  public ExistsTransactionCategoryException()
    : base(message) { }
}
