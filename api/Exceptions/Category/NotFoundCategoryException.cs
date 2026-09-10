namespace api.Exceptions;

public class NotFoundCategoryException : Exception
{
  private const string message = "Category not found.";

  public NotFoundCategoryException()
    : base(message) { }
}
