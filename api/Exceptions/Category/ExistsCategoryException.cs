namespace api.Exceptions;

public class ExistsCategoryException : Exception
{
  private const string message = "Category already exists.";

  public ExistsCategoryException()
    : base(message) { }
}
