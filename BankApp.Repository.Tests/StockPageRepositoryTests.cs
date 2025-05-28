using BankApi.Data;
using BankApi.Repositories.Impl;
using BankApi.Repositories.Impl.Stocks;
using Common.Models;
using Common.Models.Trading;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Xunit;

namespace BankApp.Repository.Tests
{
    [SupportedOSPlatform("windows10.0.26100.0")]
    public class StockPageRepositoryTests
    {
        private readonly DbContextOptions<ApiDbContext> _options;

        public StockPageRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ApiDbContext CreateContext() => new(_options);

        [Fact]
        public async Task AddOrUpdateUserStockAsync_Should_Add_New_Stock()
        {
            using var context = CreateContext();
            var user = new User { CNP = "123" };
            var stock = new Stock
            {
                Name = "AAPL",
                Price = 150,
                Quantity = 100,
                NewsArticles = []
            };
            await context.Users.AddAsync(user);
            await context.Stocks.AddAsync(stock);
            await context.SaveChangesAsync();

            var repo = new StockPageRepository(context);
            await repo.AddOrUpdateUserStockAsync("123", "AAPL", 10);

            var result = await context.UserStocks.FirstOrDefaultAsync();
            result.Should().NotBeNull();
            result!.Quantity.Should().Be(10);
        }

        [Fact]
        public async Task AddOrUpdateUserStockAsync_Should_Update_Existing_Stock()
        {
            using var context = CreateContext();
            var stock = new Stock
            {
                Name = "GOOG",
                Price = 200,
                Quantity = 50,
                NewsArticles = []
            };
            var user = new User { CNP = "999" };
            var userStock = new UserStock { UserCnp = "999", StockName = "GOOG", Quantity = 5, Stock = stock, User = user };

            await context.Users.AddAsync(user);
            await context.Stocks.AddAsync(stock);
            await context.UserStocks.AddAsync(userStock);
            await context.SaveChangesAsync();

            var repo = new StockPageRepository(context);
            await repo.AddOrUpdateUserStockAsync("999", "GOOG", 20);

            var result = await context.UserStocks.FirstAsync();
            result.Quantity.Should().Be(20);
        }

        [Fact]
        public async Task AddStockValueAsync_Should_Add_Price_Record()
        {
            using var context = CreateContext();
            var repo = new StockPageRepository(context);

            await repo.AddStockValueAsync("TSLA", 420m);

            var value = await context.StockValues.FirstOrDefaultAsync();
            value.Should().NotBeNull();
            value!.Price.Should().Be(420m);
        }

        [Fact]
        public async Task GetStockAsync_Should_Return_Stock()
        {
            using var context = CreateContext();
            await context.Stocks.AddAsync(new Stock
            {
                Name = "NFLX",
                Price = 300,
                Quantity = 75,
                NewsArticles = []
            });
            await context.SaveChangesAsync();

            var repo = new StockPageRepository(context);
            var result = await repo.GetStockAsync("NFLX");

            result.Should().NotBeNull();
            result.Name.Should().Be("NFLX");
        }

        [Fact]
        public async Task GetStockAsync_Should_Throw_When_Not_Found()
        {
            using var context = CreateContext();
            var repo = new StockPageRepository(context);

            Func<Task> act = async () => await repo.GetStockAsync("XYZ");

            await act.Should().ThrowAsync<Exception>().WithMessage("Stock not found.");
        }

        [Fact]
        public async Task GetUserStockAsync_Should_Return_Existing()
        {
            using var context = CreateContext();
            var stock = new Stock { Name = "AMZN", Price = 100, Quantity = 10, NewsArticles = [] };
            var userStock = new UserStock { UserCnp = "001", StockName = "AMZN", Quantity = 12, Stock = stock };
            await context.Stocks.AddAsync(stock);
            await context.UserStocks.AddAsync(userStock);
            await context.SaveChangesAsync();

            var repo = new StockPageRepository(context);
            var result = await repo.GetUserStockAsync("001", "AMZN");

            result.Quantity.Should().Be(12);
        }

        [Fact]
        public async Task GetUserStockAsync_Should_Create_New_When_Not_Found()
        {
            using var context = CreateContext();
            var repo = new StockPageRepository(context);

            var result = await repo.GetUserStockAsync("002", "MSFT");

            result.Should().NotBeNull();
            result.Quantity.Should().Be(0);
            result.StockName.Should().Be("MSFT");
        }

        [Fact]
        public async Task GetStockHistoryAsync_Should_Return_All_Prices()
        {
            using var context = CreateContext();
            var now = DateTime.UtcNow;
            await context.StockValues.AddRangeAsync(
            [
                new StockValue { StockName = "GOOG", Price = 100, DateTime = now.AddDays(-2) },
                new StockValue { StockName = "GOOG", Price = 200, DateTime = now.AddDays(-1) },
                new StockValue { StockName = "TSLA", Price = 300, DateTime = now }
            ]);
            await context.SaveChangesAsync();

            var repo = new StockPageRepository(context);
            var history = await repo.GetStockHistoryAsync("GOOG");

            history.Should().HaveCount(2).And.Contain([100, 200]);
        }

        [Fact]
        public async Task GetOwnedStocksAsync_Should_Return_Quantity()
        {
            using var context = CreateContext();
            await context.UserStocks.AddAsync(new UserStock { UserCnp = "333", StockName = "META", Quantity = 7 });
            await context.SaveChangesAsync();

            var repo = new StockPageRepository(context);
            var quantity = await repo.GetOwnedStocksAsync("333", "META");

            quantity.Should().Be(7);
        }

        [Fact]
        public async Task GetOwnedStocksAsync_Should_Return_Zero_When_NotFound()
        {
            using var context = CreateContext();
            var repo = new StockPageRepository(context);

            var quantity = await repo.GetOwnedStocksAsync("abc", "unknown");

            quantity.Should().Be(0);
        }

        [Fact]
        public async Task GetFavoriteAsync_Should_Return_True_If_Exists()
        {
            using var context = CreateContext();
            await context.FavoriteStocks.AddAsync(new FavoriteStock { UserCNP = "u1", StockName = "AMD" });
            await context.SaveChangesAsync();

            var repo = new StockPageRepository(context);
            var result = await repo.GetFavoriteAsync("u1", "AMD");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetFavoriteAsync_Should_Return_False_If_Not_Exists()
        {
            using var context = CreateContext();
            var repo = new StockPageRepository(context);

            var result = await repo.GetFavoriteAsync("u2", "NVDA");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ToggleFavoriteAsync_Should_Add_When_True()
        {
            using var context = CreateContext();
            var repo = new StockPageRepository(context);

            await repo.ToggleFavoriteAsync("u1", "BABA", true);

            (await context.FavoriteStocks.AnyAsync(f => f.UserCNP == "u1" && f.StockName == "BABA")).Should().BeTrue();
        }

        [Fact]
        public async Task ToggleFavoriteAsync_Should_Remove_When_False()
        {
            using var context = CreateContext();
            await context.FavoriteStocks.AddAsync(new FavoriteStock { UserCNP = "u2", StockName = "INTC" });
            await context.SaveChangesAsync();

            var repo = new StockPageRepository(context);
            await repo.ToggleFavoriteAsync("u2", "INTC", false);

            (await context.FavoriteStocks.AnyAsync()).Should().BeFalse();
        }
    }
}
