using api.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace api.Tests.Services;

public class NormalizePersonEmailsMigrationTests
{
  [Fact]
  public void Up_checks_for_case_collisions_before_lowercasing_existing_emails()
  {
    var builder = new MigrationBuilder("Npgsql.EntityFrameworkCore.PostgreSQL");

    new TestableNormalizePersonEmails().ApplyUp(builder);

    var operation = Assert.Single(builder.Operations.OfType<SqlOperation>());
    var collisionCheck = operation.Sql.IndexOf("HAVING COUNT(*) > 1", StringComparison.Ordinal);
    var update = operation.Sql.IndexOf("UPDATE persons", StringComparison.Ordinal);

    Assert.True(collisionCheck >= 0);
    Assert.True(update > collisionCheck);
    Assert.Contains("SET email = LOWER(email)", operation.Sql);
    Assert.Contains("RAISE EXCEPTION", operation.Sql);
  }

  private sealed class TestableNormalizePersonEmails : NormalizePersonEmails
  {
    public void ApplyUp(MigrationBuilder migrationBuilder)
    {
      Up(migrationBuilder);
    }
  }
}
