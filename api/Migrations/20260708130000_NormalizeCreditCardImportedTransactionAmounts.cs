using api.Models.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DatabaseContext))]
    [Migration("20260708130000_NormalizeCreditCardImportedTransactionAmounts")]
    public partial class NormalizeCreditCardImportedTransactionAmounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE transactions AS transaction
                SET
                    amount = -transaction.amount,
                    updated_at = NOW()
                FROM transactions_import AS transaction_import
                INNER JOIN files_processing AS file_processing
                    ON file_processing.id = transaction_import.file_processing_id
                INNER JOIN files AS file
                    ON file.id = file_processing.file_id
                WHERE
                    transaction.id = transaction_import.transaction_id
                    AND file.category = 0
                    AND transaction.amount > 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
