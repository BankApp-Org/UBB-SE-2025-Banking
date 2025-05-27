using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankApi.Migrations
{
    /// <inheritdoc />
    public partial class MakeStocksAndNewsArticlesMtoN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseStocks_NewsArticles_NewsArticleArticleId",
                table: "BaseStocks");

            migrationBuilder.DropIndex(
                name: "IX_BaseStocks_NewsArticleArticleId",
                table: "BaseStocks");

            migrationBuilder.DropColumn(
                name: "NewsArticleArticleId",
                table: "BaseStocks");

            migrationBuilder.CreateTable(
                name: "NewsArticleStocks",
                columns: table => new
                {
                    ArticleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StockId = table.Column<int>(type: "int", nullable: false),
                    NewsArticlesArticleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RelatedStocksId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticleStocks", x => new { x.ArticleId, x.StockId });
                    table.ForeignKey(
                        name: "FK_NewsArticleStocks_BaseStocks_RelatedStocksId",
                        column: x => x.RelatedStocksId,
                        principalTable: "BaseStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NewsArticleStocks_BaseStocks_StockId",
                        column: x => x.StockId,
                        principalTable: "BaseStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NewsArticleStocks_NewsArticles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "NewsArticles",
                        principalColumn: "ArticleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NewsArticleStocks_NewsArticles_NewsArticlesArticleId",
                        column: x => x.NewsArticlesArticleId,
                        principalTable: "NewsArticles",
                        principalColumn: "ArticleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticleStocks_NewsArticlesArticleId",
                table: "NewsArticleStocks",
                column: "NewsArticlesArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticleStocks_RelatedStocksId",
                table: "NewsArticleStocks",
                column: "RelatedStocksId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsArticleStocks_StockId",
                table: "NewsArticleStocks",
                column: "StockId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsArticleStocks");

            migrationBuilder.AddColumn<string>(
                name: "NewsArticleArticleId",
                table: "BaseStocks",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseStocks_NewsArticleArticleId",
                table: "BaseStocks",
                column: "NewsArticleArticleId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseStocks_NewsArticles_NewsArticleArticleId",
                table: "BaseStocks",
                column: "NewsArticleArticleId",
                principalTable: "NewsArticles",
                principalColumn: "ArticleId");
        }
    }
}
