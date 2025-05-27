using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankApi.Migrations
{
    /// <inheritdoc />
    public partial class OneToOneLoanAndLoanRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "ApplicationDate",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "RepaymentDate",
                table: "LoanRequests");

            migrationBuilder.AddColumn<int>(
                name: "LoanRequestId",
                table: "Loans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Loans_LoanRequestId",
                table: "Loans",
                column: "LoanRequestId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_LoanRequests_LoanRequestId",
                table: "Loans",
                column: "LoanRequestId",
                principalTable: "LoanRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_LoanRequests_LoanRequestId",
                table: "Loans");

            migrationBuilder.DropIndex(
                name: "IX_Loans_LoanRequestId",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "LoanRequestId",
                table: "Loans");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "LoanRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApplicationDate",
                table: "LoanRequests",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<DateTime>(
                name: "RepaymentDate",
                table: "LoanRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
