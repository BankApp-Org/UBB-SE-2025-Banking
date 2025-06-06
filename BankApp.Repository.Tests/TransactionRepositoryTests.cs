using BankApi.Data;
using BankApi.Repositories.Impl;
using BankApi.Repositories.Impl.Stocks;
using Common.Models;
using Common.Models.Trading;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BankApp.Repository.Tests
{
    [SupportedOSPlatform("windows10.0.26100.0")]
    [TestClass]
    public class TransactionRepositoryTests
    {
        private readonly DbContextOptions<ApiDbContext> _dbOptions;

        public TransactionRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }

        private ApiDbContext CreateContext() => new(_dbOptions);

        [TestMethod]
        public async Task GetAllTransactions_Should_Return_All_With_Authors()
        {
            using var context = CreateContext();

            var user = new User { Id = 1, CNP = "111" };
            var txn = new StockTransaction
            {
                Id = 1,
                StockName = "Apple",
                StockSymbol = "AAPL",
                Type = "BUY",
                Amount = 10,
                PricePerStock = 150,
                Date = DateTime.UtcNow,
                Author = user,
                AuthorCNP = "111"
            };

            await context.Users.AddAsync(user);
            await context.TransactionLogTransactions.AddAsync(txn);
            await context.SaveChangesAsync();

            var repo = new StockTransactionRepository(context);
            var result = await repo.getAllTransactions();

            result.Should().ContainSingle();
            result[0].Author.Should().NotBeNull();
            result[0].StockName.Should().Be("Apple");
        }

        [TestMethod]
        public async Task GetByFilterCriteriaAsync_Should_Filter_By_StockName()
        {
            using var context = CreateContext();
            await context.TransactionLogTransactions.AddRangeAsync(
                new StockTransaction
                {
                    Id = 1,
                    StockName = "TESLA",
                    StockSymbol = "TSL",
                    AuthorCNP = "123",
                    Type = "BUY",
                    Amount = 5,
                    PricePerStock = 100,
                    Date = DateTime.UtcNow,
                    Author = new User() { CNP = "123" }
                },

                new StockTransaction
                {
                    Id = 2,
                    StockName = "APPLE",
                    StockSymbol = "AAPL",
                    AuthorCNP = "456",
                    Type = "BUY",
                    Amount = 5,
                    PricePerStock = 100,
                    Date = DateTime.UtcNow,
                    Author = new User() { CNP = "456" }
                }
            );
            await context.SaveChangesAsync();

            var repo = new StockTransactionRepository(context);
            var result = await repo.GetByFilterCriteriaAsync(new StockTransactionFilterCriteria { StockName = "APPLE" });

            result.Should().ContainSingle(t => t.StockName == "APPLE");
        }

        [TestMethod]
        public async Task GetByFilterCriteriaAsync_Should_Throw_On_Null()
        {
            using var context = CreateContext();
            var repo = new StockTransactionRepository(context);

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => repo.GetByFilterCriteriaAsync(null!));
        }

        [TestMethod]
        public async Task GetByFilterCriteriaAsync_Should_Filter_By_Type_And_Value_And_Dates()
        {
            using var context = CreateContext();

            var now = DateTime.UtcNow;

            await context.TransactionLogTransactions.AddRangeAsync(
                new StockTransaction
                {
                    StockName = "GOOGLE",
                    StockSymbol = "GOOG",
                    AuthorCNP = "123",
                    Type = "BUY",
                    Amount = 1,
                    PricePerStock = 1000,
                    Date = now,
                    Author = new User() { CNP = "123" }
                },
                new StockTransaction
                {
                    StockName = "GOOGLE",
                    StockSymbol = "GOOG",
                    AuthorCNP = "456",
                    Type = "SELL",
                    Amount = 2,
                    PricePerStock = 200,
                    Date = now.AddDays(-1),
                    Author = new User() { CNP = "456" }
                }
            );
            await context.SaveChangesAsync();

            var repo = new StockTransactionRepository(context);
            var criteria = new StockTransactionFilterCriteria
            {
                Type = "SELL",
                MinTotalValue = 300,
                StartDate = now.AddDays(-2),
                EndDate = now
            };

            var result = await repo.GetByFilterCriteriaAsync(criteria);
            result.Should().ContainSingle(t => t.Type == "SELL");
        }

        [TestMethod]
        public async Task AddTransactionAsync_Should_Add_When_Stock_Exists_And_User_Provided()
        {
            using var context = CreateContext();

            var user = new User { Id = 1, CNP = "999" };
            var stock = new BaseStock { Id = 1, Name = "Microsoft" };

            await context.Users.AddAsync(user);
            await context.BaseStocks.AddAsync(stock);
            await context.SaveChangesAsync();

            var transaction = new StockTransaction
            {
                StockName = "Microsoft",
                StockSymbol = "MSFT",
                Type = "BUY",
                Amount = 3,
                PricePerStock = 300,
                Date = DateTime.UtcNow,
                Author = user,
                AuthorCNP = user.CNP,
            };

            var repo = new StockTransactionRepository(context);
            await repo.AddTransactionAsync(transaction);

            context.TransactionLogTransactions.Count().Should().Be(1);
            context.TransactionLogTransactions.First().Author.CNP.Should().Be("999");
        }

        [TestMethod]
        public async Task AddTransactionAsync_Should_Throw_When_Transaction_Null()
        {
            using var context = CreateContext();
            var repo = new StockTransactionRepository(context);

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => repo.AddTransactionAsync(null!));
        }

        [TestMethod]
        public async Task AddTransactionAsync_Should_Throw_When_Stock_Does_Not_Exist()
        {
            using var context = CreateContext();

            var user = new User { Id = 2, CNP = "123" };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var txn = new StockTransaction
            {
                StockName = "NONEXISTENT",
                StockSymbol = "NAN",
                Type = "SELL",
                Amount = 2,
                PricePerStock = 100,
                Date = DateTime.UtcNow,
                Author = user,
                AuthorCNP = user.CNP,
            };

            var repo = new StockTransactionRepository(context);
            Func<Task> act = () => repo.AddTransactionAsync(txn);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Stock with name*does not exist*");
        }

        [TestMethod]
        public async Task AddTransactionAsync_Should_Reset_Id()
        {
            using var context = CreateContext();

            var stock = new BaseStock { Id = 3, Name = "AMD" };
            await context.BaseStocks.AddAsync(stock);
            await context.SaveChangesAsync();

            var txn = new StockTransaction
            {
                Id = 999,
                StockName = "AMD",
                StockSymbol = "AMD",
                Type = "BUY",
                Amount = 1,
                PricePerStock = 80,
                Date = DateTime.UtcNow,
                Author = new User { CNP = "123" },
                AuthorCNP = "123",
            };

            var repo = new StockTransactionRepository(context);
            await repo.AddTransactionAsync(txn);

            var savedTxn = await context.TransactionLogTransactions.FirstAsync();
            savedTxn.Id.Should().NotBe(999); // EF will generate a new one
        }
    }
}
