using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankApi.Migrations
{
    /// <inheritdoc />
    public partial class Cascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsArticleStock_NewsArticles_ArticleId",
                table: "NewsArticleStock");

            migrationBuilder.AddForeignKey(
                name: "FK_NewsArticleStock_NewsArticles_ArticleId",
                table: "NewsArticleStock",
                column: "ArticleId",
                principalTable: "NewsArticles",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NewsArticleStock_NewsArticles_ArticleId",
                table: "NewsArticleStock");

            migrationBuilder.AddForeignKey(
                name: "FK_NewsArticleStock_NewsArticles_ArticleId",
                table: "NewsArticleStock",
                column: "ArticleId",
                principalTable: "NewsArticles",
                principalColumn: "ArticleId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
