using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankApi.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureMessagePolymorphism : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Messages",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Messages",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Currency",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestMessage_Currency",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestMessage_Description",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestMessage_Status",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Messages",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransferMessage_Amount",
                table: "Messages",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransferMessage_Currency",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferMessage_Description",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferMessage_Status",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BillSplitMessageId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImageMessageId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TextMessageId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransferMessageId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BillSplitMessageId",
                table: "AspNetUsers",
                column: "BillSplitMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ImageMessageId",
                table: "AspNetUsers",
                column: "ImageMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TextMessageId",
                table: "AspNetUsers",
                column: "TextMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TransferMessageId",
                table: "AspNetUsers",
                column: "TransferMessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Messages_BillSplitMessageId",
                table: "AspNetUsers",
                column: "BillSplitMessageId",
                principalTable: "Messages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Messages_ImageMessageId",
                table: "AspNetUsers",
                column: "ImageMessageId",
                principalTable: "Messages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Messages_TextMessageId",
                table: "AspNetUsers",
                column: "TextMessageId",
                principalTable: "Messages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Messages_TransferMessageId",
                table: "AspNetUsers",
                column: "TransferMessageId",
                principalTable: "Messages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Messages_BillSplitMessageId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Messages_ImageMessageId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Messages_TextMessageId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Messages_TransferMessageId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_BillSplitMessageId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ImageMessageId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TextMessageId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TransferMessageId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "RequestMessage_Currency",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "RequestMessage_Description",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "RequestMessage_Status",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "TransferMessage_Amount",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "TransferMessage_Currency",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "TransferMessage_Description",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "TransferMessage_Status",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "BillSplitMessageId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ImageMessageId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TextMessageId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TransferMessageId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(13)",
                oldMaxLength: 13);
        }
    }
}
