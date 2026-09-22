using api.Models.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260922120000_NormalizePersonEmails")]
    public partial class NormalizePersonEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT LOWER(email)
                        FROM persons
                        WHERE deleted_at IS NULL
                        GROUP BY LOWER(email)
                        HAVING COUNT(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Cannot normalize person emails: active emails differ only by letter casing.';
                    END IF;
                END $$;

                UPDATE persons
                SET email = LOWER(email)
                WHERE email <> LOWER(email);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
