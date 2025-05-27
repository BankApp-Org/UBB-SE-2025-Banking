using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankApi.Migrations
{
    /// <inheritdoc />
    public partial class MakeM2MRelationSpecific : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsArticleStocks_BaseStocks_RelatedStocksId",
                table: "NewsArticleStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_NewsArticleStocks_BaseStocks_StockId",
                table: "NewsArticleStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_NewsArticleStocks_NewsArticles_ArticleId",
                table: "NewsArticleStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_NewsArticleStocks_NewsArticles_NewsArticlesArticleId",
                table: "NewsArticleStocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NewsArticleStocks",
                table: "NewsArticleStocks");

            migrationBuilder.DropIndex(
                name: "IX_NewsArticleStocks_NewsArticlesArticleId",
                table: "NewsArticleStocks");

            migrationBuilder.DropIndex(
                name: "IX_NewsArticleStocks_RelatedStocksId",
                table: "NewsArticleStocks");

            migrationBuilder.DropColumn(
                name: "NewsArticlesArticleId",
                table: "NewsArticleStocks");

            migrationBuilder.DropColumn(
                name: "RelatedStocksId",
                table: "NewsArticleStocks");

            migrationBuilder.RenameTable(
                name: "NewsArticleStocks",
                newName: "NewsArticleStock");

            migrationBuilder.RenameIndex(
                name: "IX_NewsArticleStocks_StockId",
                table: "NewsArticleStock",
                newName: "IX_NewsArticleStock_StockId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NewsArticleStock",
                table: "NewsArticleStock",
                columns: ["ArticleId", "StockId"]);

            migrationBuilder.AddForeignKey(
                name: "FK_NewsArticleStock_BaseStocks_StockId",
                table: "NewsArticleStock",
                column: "StockId",
                principalTable: "BaseStocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NewsArticleStock_NewsArticles_ArticleId",
                table: "NewsArticleStock",
                column: "ArticleId",
                principalTable: "NewsArticles",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsArticleStock_BaseStocks_StockId",
                table: "NewsArticleStock");

            migrationBuilder.DropForeignKey(
                name: "FK_NewsArticleStock_NewsArticles_ArticleId",
                table: "NewsArticleStock");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NewsArticleStock",
                table: "NewsArticleStock");

            migrationBuilder.RenameTable(
                name: "NewsArticleStock",
                newName: "NewsArticleStocks");

            migrationBuilder.RenameIndex(
                name: "IX_NewsArticleStock_StockId",
                table: "NewsArticleStocks",
                newName: "IX_NewsArticleStocks_StockId");

            migrationBuilder.AddColumn<string>(
                name: "NewsArticlesArticleId",
                table: "NewsArticleStocks",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RelatedStocksId",
                table: "NewsArticleStocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_NewsArticleStocks",
                table: "NewsArticleStocks",
                columns: ["ArticleId", "StockId"]);

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticleStocks_NewsArticlesArticleId",
                table: "NewsArticleStocks",
                column: "NewsArticlesArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticleStocks_RelatedStocksId",
                table: "NewsArticleStocks",
                column: "RelatedStocksId");

            migrationBuilder.AddForeignKey(
                name: "FK_NewsArticleStocks_BaseStocks_RelatedStocksId",
                table: "NewsArticleStocks",
                column: "RelatedStocksId",
                principalTable: "BaseStocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NewsArticleStocks_BaseStocks_StockId",
                table: "NewsArticleStocks",
                column: "StockId",
                principalTable: "BaseStocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NewsArticleStocks_NewsArticles_ArticleId",
                table: "NewsArticleStocks",
                column: "ArticleId",
                principalTable: "NewsArticles",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NewsArticleStocks_NewsArticles_NewsArticlesArticleId",
                table: "NewsArticleStocks",
                column: "NewsArticlesArticleId",
                principalTable: "NewsArticles",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
