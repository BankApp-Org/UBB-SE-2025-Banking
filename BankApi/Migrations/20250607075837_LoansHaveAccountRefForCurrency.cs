using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankApi.Migrations
{
    /// <inheritdoc />
    public partial class LoansHaveAccountRefForCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountIban",
                table: "LoanRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountIban",
                table: "LoanRequests");
        }
    }
}
