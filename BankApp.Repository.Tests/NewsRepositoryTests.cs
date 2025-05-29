using BankApi.Data;
using BankApi.Repositories.Impl;
using Common.Models;
using Common.Models.Trading;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Xunit;

namespace BankApp.Repository.Tests
{
    [SupportedOSPlatform("windows10.0.26100.0")]

    public class NewsRepositoryTests
    {
        private readonly DbContextOptions<ApiDbContext> _dbOptions;

        public NewsRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private ApiDbContext CreateContext() => new(_dbOptions);

        [Fact]
        public async Task AddNewsArticleAsync_Should_Add_Article_When_Valid()
        {
            // Arrange
            using var context = CreateContext();

            var stock = new Stock
            {
                Name = "TEST",
                Price = 100,
                Quantity = 10,
                NewsArticles = []
            };
            var author = new User { Id = 123 };

            await context.Stocks.AddAsync(stock);
            await context.Users.AddAsync(author);
            await context.SaveChangesAsync();

            var article = new NewsArticle
            {
                Author = author,
                RelatedStocks = [stock],
                Title = "Test Title",
                Content = "Test Content",
                Category = "Test Category",
                Source = "http://source",
                Summary = "Test Summary"
            };

            var repo = new NewsRepository(context);

            // Act
            await repo.AddNewsArticleAsync(article);

            // Assert
            var inserted = await context.NewsArticles.FirstOrDefaultAsync();
            inserted.Should().NotBeNull();
            inserted!.Title.Should().Be("Test Title");
            inserted.RelatedStocks.Should().ContainSingle(s => s.Name == "TEST");
        }

        [Fact]
        public async Task AddNewsArticleAsync_Should_Throw_When_Stock_Not_Found()
        {
            using var context = CreateContext();

            var author = new User { Id = 123 };
            await context.Users.AddAsync(author);
            await context.SaveChangesAsync();

            var article = new NewsArticle
            {
                Author = author,
                RelatedStocks = [new Stock { Name = "MISSING", Price = 100, Quantity = 10, NewsArticles = [] }],
            };

            var repo = new NewsRepository(context);

            Func<Task> act = async () => await repo.AddNewsArticleAsync(article);

            await act.Should().ThrowAsync<Exception>()
            .WithMessage("Error while adding news article.");
        }

        [Fact]
        public async Task AddNewsArticleAsync_Should_Call_Update_When_Article_Exists()
        {
            using var context = CreateContext();

            var stock = new Stock
            {
                Name = "S1",
                Price = 0,
                Quantity = 0,
                NewsArticles = [],
            };
            var author = new User { Id = 1 };
            var articleId = Guid.NewGuid().ToString();

            var existing = new NewsArticle
            {
                ArticleId = articleId,
                Author = author,
                Title = "Old",
                RelatedStocks = [stock],
                Content = "Content",
                Category = "Category",
                Source = "Source",
                Summary = "Summary"
            };

            await context.Users.AddAsync(author);
            await context.Stocks.AddAsync(stock);
            await context.NewsArticles.AddAsync(existing);
            await context.SaveChangesAsync();

            var repo = new NewsRepository(context);

            var updated = new NewsArticle
            {
                ArticleId = articleId,
                Author = author,
                Title = "New",
                RelatedStocks = [stock],
                Category = "Category",
                Source = "Source",
                Summary = "Summary"
            };

            await repo.AddNewsArticleAsync(updated);

            var result = await context.NewsArticles.FirstAsync();
            result.Title.Should().Be("New");
        }

        [Fact]
        public async Task UpdateNewsArticleAsync_Should_Update_Fields()
        {
            using var context = CreateContext();

            var article = new NewsArticle
            {
                ArticleId = Guid.NewGuid().ToString(),
                Title = "Original",
                Content = "Old",
                Author = new User { Id = 123 },
                Category = "Test Category",
                Source = "http://source",
                Summary = "Test Summary"
            };
            await context.Users.AddAsync(article.Author);
            await context.NewsArticles.AddAsync(article);
            await context.SaveChangesAsync();

            article.Title = "Updated";
            article.Content = "New";

            var repo = new NewsRepository(context);

            // Act
            await repo.UpdateNewsArticleAsync(article);

            // Assert
            var updated = await context.NewsArticles.FirstOrDefaultAsync(a => a.ArticleId == article.ArticleId);
            updated!.Title.Should().Be("Updated");
            updated.Content.Should().Be("New");
        }

        [Fact]
        public async Task UpdateNewsArticleAsync_Should_Throw_When_NotFound()
        {
            using var context = CreateContext();
            var repo = new NewsRepository(context);

            var article = new NewsArticle { ArticleId = "invalid" };

            Func<Task> act = async () => await repo.UpdateNewsArticleAsync(article);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Error while updating news article.");
        }

        [Fact]
        public async Task DeleteNewsArticleAsync_Should_Delete_When_Found()
        {
            using var context = CreateContext();

            var article = new NewsArticle
            {
                ArticleId = "123",
                Content = "Test Content",
                Title = "Test Title",
                Category = "Test Category",
                Source = "http://source",
                Summary = "Test Summary"
            };
            await context.NewsArticles.AddAsync(article);
            await context.SaveChangesAsync();

            var repo = new NewsRepository(context);
            await repo.DeleteNewsArticleAsync("123");

            (await context.NewsArticles.AnyAsync()).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteNewsArticleAsync_Should_Throw_When_NotFound()
        {
            using var context = CreateContext();
            var repo = new NewsRepository(context);

            Func<Task> act = async () => await repo.DeleteNewsArticleAsync("not-exist");

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Error while deleting news article.");
        }

        [Fact]
        public async Task GetNewsArticleByIdAsync_Should_Return_Article()
        {
            using var context = CreateContext();

            var article = new NewsArticle
            {
                ArticleId = "a1",
                Author = new User { Id = 123 },
                Title = "Test Title",
                Content = "Test Content",
                Category = "Test Category",
                Source = "http://source",
                Summary = "Test Summary"
            };
            await context.Users.AddAsync(article.Author);
            await context.NewsArticles.AddAsync(article);
            await context.SaveChangesAsync();

            var repo = new NewsRepository(context);

            var result = await repo.GetNewsArticleByIdAsync("a1");

            result.Should().NotBeNull();
            result.ArticleId.Should().Be("a1");
        }

        [Fact]
        public async Task GetNewsArticleByIdAsync_Should_Throw_When_NotFound()
        {
            using var context = CreateContext();
            var repo = new NewsRepository(context);

            Func<Task> act = async () => await repo.GetNewsArticleByIdAsync("not-there");

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Error while retrieving news article.");
        }

        [Fact]
        public async Task GetAllNewsArticlesAsync_Should_Return_All()
        {
            using var context = CreateContext();

            await context.NewsArticles.AddRangeAsync(
            [
                new NewsArticle
                {
                    ArticleId = "a",
                    Content = "Content",
                    Author = new User { Id = 1, CNP = "123" },
                    Title = "1",
                    Category = "cat",
                    Source = "src",
                    Summary = "sum"
                },
                new NewsArticle
                {
                    ArticleId = "b",
                    Content = "Content",
                    Author = new User { Id = 2, CNP = "456" },
                    Title = "2",
                    Category = "cat",
                    Source = "src",
                    Summary = "sum"
                }
            ]);
            await context.SaveChangesAsync();

            var repo = new NewsRepository(context);
            var result = await repo.GetAllNewsArticlesAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task MarkArticleAsReadAsync_Should_Mark_Read()
        {
            using var context = CreateContext();

            var article = new NewsArticle
            {
                ArticleId = "m1",
                Author = new User { Id = 1 },
                IsRead = false,
                Category = "cat",
                Source = "src",
                Summary = "sum",
                Title = "title",
                Content = "c"
            };

            await context.Users.AddAsync(article.Author);
            await context.NewsArticles.AddAsync(article);
            await context.SaveChangesAsync();

            var repo = new NewsRepository(context);

            await repo.MarkArticleAsReadAsync("m1");

            (await context.NewsArticles.FindAsync("m1"))!.IsRead.Should().BeTrue();
        }

        [Fact]
        public async Task GetNewsArticlesByAuthorCNPAsync_Should_Filter()
        {
            using var context = CreateContext();

            await context.NewsArticles.AddRangeAsync(
            [
                new NewsArticle
                {
                    ArticleId = "1",
                    AuthorCNP = "123",
                    Content = "Content",
                    Title = "A",
                    Author = new User { Id = 1, CNP = "123" },
                    Category = "c",
                    Source = "s",
                    Summary = "sum"
                },
                new NewsArticle
                {
                    ArticleId = "2",
                    AuthorCNP = "456",
                    Content = "Content",
                    Title = "B",
                    Author = new User { Id = 2, CNP = "456" },
                    Category = "c",
                    Source = "s",
                    Summary = "sum"
                }
            ]);
            await context.SaveChangesAsync();

            var repo = new NewsRepository(context);

            var result = await repo.GetNewsArticlesByAuthorCNPAsync("123");

            result.Should().ContainSingle(a => a.ArticleId == "1");
        }

        [Fact]
        public async Task UpdateNewsArticleAsync_Should_Preserve_Inner_Exception()
        {
            using var context = CreateContext();
            var repo = new NewsRepository(context);
            var article = new NewsArticle { ArticleId = "bad" };

            var ex = await Assert.ThrowsAsync<Exception>(() => repo.UpdateNewsArticleAsync(article));
            ex.InnerException.Should().BeOfType<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetNewsArticlesByCategoryAsync_Should_Return_Expected_Articles()
        {
            using var context = CreateContext();

            var category = "Finance";
            var author = new User { Id = 1 };

            var articles = new List<NewsArticle>
        {
            new() {
                ArticleId = "a1",
                Category = category,
                Title = "Finance News 1",
                Author = author,
                RelatedStocks = [],
                Source = "src", Summary = "sum", Content = "c"
            },
            new() {
                ArticleId = "a2",
                Category = "Tech",
                Title = "Tech News",
                Author = author,
                RelatedStocks = [],
                Source = "src", Summary = "sum", Content = "c"
            }
        };

            await context.Users.AddAsync(author);
            await context.NewsArticles.AddRangeAsync(articles);
            await context.SaveChangesAsync();

            var repo = new NewsRepository(context);

            var result = await repo.GetNewsArticlesByCategoryAsync(category);

            result.Should().HaveCount(1);
            result[0].Title.Should().Be("Finance News 1");
        }

        [Fact]
        public async Task GetNewsArticlesByStockAsync_Should_Return_Expected_Articles()
        {
            using var context = CreateContext();

            var stock1 = new Stock
            {
                Name = "AAPL",
                Price = 150,
                Quantity = 100,
                NewsArticles = [],
            };
            var stock2 = new Stock
            {
                Name = "MSFT",
                Price = 0,
                Quantity = 0,
                NewsArticles = [],
            };
            var author = new User { Id = 2 };

            var articles = new List<NewsArticle>
        {
            new() {
                ArticleId = "s1",
                Title = "Apple Update",
                Category = "Tech",
                Author = author,
                RelatedStocks = [stock1],
                Source = "src", Summary = "sum", Content = "c"
            },
            new() {
                ArticleId = "s2",
                Title = "Microsoft News",
                Category = "Tech",
                Author = author,
                RelatedStocks = [stock2],
                Source = "src", Summary = "sum", Content = "c"
            }
        };

            await context.Users.AddAsync(author);
            await context.Stocks.AddRangeAsync(stock1, stock2);
            await context.NewsArticles.AddRangeAsync(articles);
            await context.SaveChangesAsync();

            var repo = new NewsRepository(context);

            var result = await repo.GetNewsArticlesByStockAsync("AAPL");

            result.Should().ContainSingle()
            .Which.Title.Should().Be("Apple Update");
        }

    }
}
