namespace api.Exceptions.Database;

public class NotFoundSettingsDatabaseException : Exception
{
  private const string message = "Database settings not found.";

  public NotFoundSettingsDatabaseException() 
    : base(message) { }

  public NotFoundSettingsDatabaseException(string m)
    : base(message) { }

  public NotFoundSettingsDatabaseException(string m, Exception inner)
    : base(message, inner) { }
}
