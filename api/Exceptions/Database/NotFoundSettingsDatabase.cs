namespace api.Exceptions;

public class NotFoundSettingsDatabase : Exception
{
  private const string message = "Database settings not found.";

  public NotFoundSettingsDatabase() { }

  public NotFoundSettingsDatabase(string m)
    : base(message) { }

  public NotFoundSettingsDatabase(string m, Exception inner)
    : base(message, inner) { }
}
