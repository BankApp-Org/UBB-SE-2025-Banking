using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankApi.Migrations
{
    /// <inheritdoc />
    public partial class yes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First check if the column exists with the old name
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE Name = N'TransactionTypes'
                    AND Object_ID = Object_ID(N'[BankTransactions]')
                )
                BEGIN
                    EXEC sp_rename N'[BankTransactions].[TransactionTypes]', N'TransactionType', 'COLUMN';
                END
                ELSE IF NOT EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE Name = N'TransactionType'
                    AND Object_ID = Object_ID(N'[BankTransactions]')
                )
                BEGIN
                    -- If neither column exists, add the new one
                    ALTER TABLE [BankTransactions] ADD [TransactionType] int NOT NULL DEFAULT(0);
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Check if the new name exists before trying to rename back
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE Name = N'TransactionType'
                    AND Object_ID = Object_ID(N'[BankTransactions]')
                )
                BEGIN
                    EXEC sp_rename N'[BankTransactions].[TransactionType]', N'TransactionTypes', 'COLUMN';
                END
                ELSE IF NOT EXISTS (
                    SELECT 1 
                    FROM sys.columns 
                    WHERE Name = N'TransactionTypes'
                    AND Object_ID = Object_ID(N'[BankTransactions]')
                )
                BEGIN
                    -- If neither column exists, add the old one back
                    ALTER TABLE [BankTransactions] ADD [TransactionTypes] int NOT NULL DEFAULT(0);
                END");
        }
    }
}
