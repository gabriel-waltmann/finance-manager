using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class EnforceUniqueTransactionPersonTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_transactions_person_person_id_transaction_id",
                table: "transactions_person");

            migrationBuilder.DropIndex(
                name: "IX_transactions_person_transaction_id",
                table: "transactions_person");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_person_person_id",
                table: "transactions_person",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_person_transaction_id",
                table: "transactions_person",
                column: "transaction_id",
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_transactions_person_person_id",
                table: "transactions_person");

            migrationBuilder.DropIndex(
                name: "IX_transactions_person_transaction_id",
                table: "transactions_person");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_person_person_id_transaction_id",
                table: "transactions_person",
                columns: new[] { "person_id", "transaction_id" },
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_person_transaction_id",
                table: "transactions_person",
                column: "transaction_id");
        }
    }
}
