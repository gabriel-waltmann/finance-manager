using api.Helpers.Database;
using api.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace api.Models.Database;

public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
  public DatabaseContext CreateDbContext(string[] args)
  {
    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

    var configuration = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: false)
      .AddJsonFile($"appsettings.{environment}.json", optional: true)
      .AddEnvironmentVariables()
      .AddUserSecrets<DatabaseContextFactory>(optional: true)
      .Build();

    var databaseSettings = configuration.GetSection("Database").Get<DatabaseSettings>()
      ?? throw new InvalidOperationException("Database settings not found.");

    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
    optionsBuilder.UseNpgsql(BuildDatabaseHelper.BuildUrlString(databaseSettings));

    return new DatabaseContext(optionsBuilder.Options);
  }
}
