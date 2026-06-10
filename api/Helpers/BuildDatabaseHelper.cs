using api.Settings;

namespace api.Helpers;

public static class BuildDatabaseHelper
{
  public static string Execute(DatabaseSettings settings)
  {
    var host = settings.Host;
    var port = settings.Port;
    var name = settings.Name;
    var user = settings.User;
    var password = settings.Password;
    
    return $"Host={host};Port={port};Database={name};Username={user};Password={password}";
  }
}
