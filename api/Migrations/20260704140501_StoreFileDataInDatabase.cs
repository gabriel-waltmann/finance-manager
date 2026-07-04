using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class StoreFileDataInDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "container",
                table: "files");

            migrationBuilder.AddColumn<byte[]>(
                name: "data",
                table: "files",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "data",
                table: "files");

            migrationBuilder.AddColumn<string>(
                name: "container",
                table: "files",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
